using System;
using System.Collections.Generic;
using Npgsql;
using SimuladorDronApp.Domain;

namespace SimuladorDronApp.Data
{
    public class AccesoPostgre
    {
        private readonly string _connectionString;

        public AccesoPostgre(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InicializarBaseDeDatos()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string ddlMaster = @"
                CREATE TABLE IF NOT EXISTS tb_master_control (
                id SERIAL PRIMARY KEY,
                fecha_sistema TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                dimension_n INT NOT NULL,
                despegue_x INT NOT NULL,
                despegue_y INT NOT NULL
                );";

            string ddlDetalle = @"
                CREATE TABLE IF NOT EXISTS tb_det_log (
                id SERIAL PRIMARY KEY,
                master_id INT NOT NULL,
                paso_etiqueta INT NOT NULL,
                coordenada_x INT NOT NULL,
                coordenada_y INT NOT NULL,
                CONSTRAINT fk_master FOREIGN KEY (master_id) 
                REFERENCES tb_master_control(id) ON DELETE CASCADE
                );";

            using var cmd1 = new NpgsqlCommand(ddlMaster, conn);
            cmd1.ExecuteNonQuery();

            using var cmd2 = new NpgsqlCommand(ddlDetalle, conn);
            cmd2.ExecuteNonQuery();
        }

        public int GuardarSimulacion(int n, Coordenada despegue, List<(int paso, Coordenada coord)> secuencia)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                string insertMaster = @"
                INSERT INTO tb_master_control (dimension_n, despegue_x, despegue_y)
                VALUES (@n, @x, @y) RETURNING id;";

                using var cmdMaster = new NpgsqlCommand(insertMaster, conn, tx);
                cmdMaster.Parameters.AddWithValue("@n", n);
                cmdMaster.Parameters.AddWithValue("@x", despegue.X);
                cmdMaster.Parameters.AddWithValue("@y", despegue.Y);

                int masterId = Convert.ToInt32(cmdMaster.ExecuteScalar());

                string insertDetalle = @"
                INSERT INTO tb_det_log (master_id, paso_etiqueta, coordenada_x, coordenada_y)
                VALUES (@masterId, @paso, @cx, @cy);";

                int i = 0;
                int cantidadMovimientos = secuencia.Count;

                // RESTRICCIÓN: Bucle while controlado manualmente para persistencia
                while (i < cantidadMovimientos)
                {
                    var mov = secuencia[i];
                    int pasoOfuscado = OfuscarPaso(mov.paso);

                    using var cmdDet = new NpgsqlCommand(insertDetalle, conn, tx);
                    cmdDet.Parameters.AddWithValue("@masterId", masterId);
                    cmdDet.Parameters.AddWithValue("@paso", pasoOfuscado);
                    cmdDet.Parameters.AddWithValue("@cx", mov.coord.X);
                    cmdDet.Parameters.AddWithValue("@cy", mov.coord.Y);

                    cmdDet.ExecuteNonQuery();
                    i++;
                }

                tx.Commit();
                return masterId;
            }
            catch (Exception)
            {
                tx.Rollback();
                throw;
            }
        }

        public List<string> ObtenerReporteInverso(int masterId)
        {
            List<string> lineasReporte = new List<string>();

            string query = @"
            SELECT id, paso_etiqueta, coordenada_x, coordenada_y 
            FROM tb_det_log 
            WHERE master_id = @masterId 
            ORDER BY id DESC 
            LIMIT 5;";

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@masterId", masterId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int idDet = reader.GetInt32(0);
                int pasoOfuscado = reader.GetInt32(1);
                int cx = reader.GetInt32(2);
                int cy = reader.GetInt32(3);

                int pasoReal = ReconstruirPaso(pasoOfuscado);

                lineasReporte.Add($"[DB ID: {idDet}] -> Paso Real Decodificado: {pasoReal} en Coordenada: ({cx}, {cy})");
            }

            return lineasReporte;
        }

        private int OfuscarPaso(int paso)
        {
            if (paso % 2 == 0)
                return paso * 2; // Par multiplicado por 2
            else
                return -paso; // Impar guardado como negativo
        }

        private int ReconstruirPaso(int pasoOfuscado)
        {
            if (pasoOfuscado < 0)
                return -pasoOfuscado; // Negativo cambia signo para volver a ser Impar

            return pasoOfuscado / 2; // Positivo/Cero se divide por 2 para volver a ser Par
        }
    }
}

