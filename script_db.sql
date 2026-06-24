-- ============================================================
-- SCRIPT DDL - SIMULADOR DRON (POSTGRESQL COMPATIBLE)
-- ============================================================

-- 1. Tabla Principal: tb_master_control
CREATE TABLE IF NOT EXISTS tb_master_control (
    id SERIAL PRIMARY KEY,
    fecha_sistema TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    dimension_n INT NOT NULL,
    despegue_x INT NOT NULL,
    despegue_y INT NOT NULL
);

-- 2. Tabla Detalle: tb_det_log
CREATE TABLE IF NOT EXISTS tb_det_log (
    id SERIAL PRIMARY KEY,
    master_id INT NOT NULL,
    paso_etiqueta INT NOT NULL,
    coordenada_x INT NOT NULL,
    coordenada_y INT NOT NULL,
    CONSTRAINT fk_master FOREIGN KEY (master_id) 
        REFERENCES tb_master_control(id) ON DELETE CASCADE
);