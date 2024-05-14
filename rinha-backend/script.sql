CREATE DATABASE IF NOT EXISTS Rinha;

\c Rinha;

DO $$
BEGIN
    -- Criação da tabela Clientes
    CREATE TABLE IF NOT EXISTS Clientes
    (
        id
        SERIAL
        PRIMARY
        KEY,
        nome
        VARCHAR (250),
        limite INT,
        saldo_inicial INT,

    -- Criação de índices para Clientes
    CREATE INDEX IF NOT EXISTS idx_saldo_inicial ON Clientes (saldo_inicial);
    CREATE INDEX IF NOT EXISTS idx_limite ON Clientes (limite);

    -- Criação da tabela Transacao
    CREATE TABLE IF NOT EXISTS Transacao
    (
        id SERIAL PRIMARY KEY,
        id_cliente INT,
        valor INT,
        tipo VARCHAR (1),
        descricao VARCHAR(10),
        realizada_em TIMESTAMP,
        FOREIGN KEY(id_cliente) REFERENCES Clientes(id));

    -- Inserção de dados na tabela Clientes
    INSERT INTO Clientes (nome, limite, saldo_inicial)
    VALUES ('o barato sai caro', 1000 * 100, 0),
           ('zan corp ltda', 800 * 100, 0),
           ('les cruders', 10000 * 100, 0),
           ('padaria joia de cocaia', 100000 * 100, 0),
           ('kid mais', 5000 * 100, 0);
END $$;

-- função debitar
CREATE OR REPLACE FUNCTION debitar(id_cliente INT, valor_debitar INT, descricao VARCHAR)
RETURNS INT AS $$
DECLARE
    saldo_cliente INT;
    saldo_novo INT;
BEGIN
    -- Busca o saldo do cliente
    SELECT saldo_inicial
    INTO saldo_cliente
    FROM Clientes
    WHERE id = id_cliente LIMIT 1;

    -- Se o saldo do cliente for nulo, lança uma exceção
    IF saldo_cliente IS NULL THEN RAISE EXCEPTION 'deu errado';
END
IF;

-- Verifica se há saldo suficiente
IF saldo_cliente - valor_debitar < limite THEN
        RAISE EXCEPTION 'deu errado limite';
END IF;

-- Realiza a função
saldo_novo := saldo_cliente - valor_debitar;

-- Atualiza o saldo do cliente
UPDATE Clientes
SET saldo_inicial = saldo_novo
WHERE id = id_cliente;

-- Insere uma nova transação
INSERT INTO Transacao
    (id_cliente, valor, tipo, descricao, realizada_em)
VALUES (id_cliente, valor_debitar, 'd', descricao, CURRENT_TIMESTAMP);

-- Retorna o novo saldo
RETURN saldo_novo;
END;
$$ LANGUAGE plpgsql;
    
-------------------------------------------
    
-- função creditar
CREATE OR REPLACE FUNCTION creditar(id_cliente INT, valor_debitar INT, descricao VARCHAR)
RETURNS INT AS $$
DECLARE
    saldo_cliente INT;
saldo_novo INT;
BEGIN
    -- Busca o saldo do cliente
    SELECT saldo_inicial
    INTO saldo_cliente
    FROM Clientes
    WHERE id = id_cliente LIMIT 1;

    -- Se o saldo do cliente for nulo, lança uma exceção
    IF saldo_cliente IS NULL THEN RAISE EXCEPTION 'deu errado';
END
IF;

-- Realiza a função
saldo_novo := saldo_cliente + valor_debitar;

-- Atualiza o saldo do cliente
UPDATE Clientes
SET saldo_inicial = saldo_novo
WHERE id = id_cliente;

-- Insere uma nova transação
INSERT INTO Transacao
    (id_cliente, valor, tipo, descricao, realizada_em)
VALUES (id_cliente, valor_debitar, 'c', descricao, CURRENT_TIMESTAMP);

-- Retorna o novo saldo
RETURN saldo_novo;
END;
$$ LANGUAGE plpgsql;