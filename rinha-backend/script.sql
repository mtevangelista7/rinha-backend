CREATE DATABASE IF NOT EXISTS Rinha;

\c Rinha;

DO $$

CREATE TABLE IF NOT EXISTS Clientes (
    id INT PRIMARY KEY,
    nome VARCHAR(250),
    limite INT,
    saldo INT
);

BEGIN
  INSERT INTO Clientes (nome, limite, saldo)
  VALUES
    ('o barato sai caro', 1000 * 100, 0),
    ('zan corp ltda', 800 * 100, 0),
    ('les cruders', 10000 * 100, 0),
    ('padaria joia de cocaia', 100000 * 100, 0),
    ('kid mais', 5000 * 100, 0);
END;

CREATE TABLE IF NOT EXISTS Transacao (
    id INT PRIMARY KEY,
    id_cliente int,
    valor int,
    tipo varchar(1),
    descricao varchar(10),
    realiza_em timestamp
);


-- criar index
$$;
