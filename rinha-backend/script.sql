\c postgres;

-- Criação do banco de dados Rinha
CREATE DATABASE Rinha;

-- Conexão ao banco de dados Rinha
\c Rinha;

DO $$
BEGIN
    -- Criação da tabela Clientes
    CREATE TABLE IF NOT EXISTS Clientes
    (
        id SERIAL PRIMARY KEY,
        nome VARCHAR(250),
        limite INT,
        saldo_inicial INT
    );

    -- Criação de índices para Clientes
    CREATE INDEX IF NOT EXISTS idx_saldo_inicial ON Clientes (saldo_inicial);
    CREATE INDEX IF NOT EXISTS idx_limite ON Clientes (limite);

    -- Criação da tabela Transacao
    CREATE TABLE IF NOT EXISTS Transacao
    (
        id SERIAL PRIMARY KEY,
        id_cliente INT,
        valor INT,
        tipo VARCHAR(1),
        descricao VARCHAR(10),
        realizada_em TIMESTAMP,
        FOREIGN KEY(id_cliente) REFERENCES Clientes(id)
    );

    -- Inserção de dados na tabela Clientes
    INSERT INTO Clientes (nome, limite, saldo_inicial)
    VALUES 
        ('o barato sai caro', 1000 * 100, 0),
        ('zan corp ltda', 800 * 100, 0),
        ('les cruders', 10000 * 100, 0),
        ('padaria joia de cocaia', 100000 * 100, 0),
        ('kid mais', 5000 * 100, 0);

END $$;

-- buscar extrato
CREATE OR REPLACE FUNCTION RetornaExtrato(id_cli INT)
RETURNS TABLE (
    valor INT,
    tipo VARCHAR,
    descricao VARCHAR,
    realizada_em TIMESTAMP,
    saldo_inicial INT,
    limite INT
) AS $$
BEGIN
RETURN QUERY
SELECT t.valor, t.tipo, t.descricao, t.realizada_em, c.saldo_inicial, c.limite
FROM Transacao t
         INNER JOIN Clientes c ON t.id_cliente = c.id
WHERE t.id_cliente = id_cli
ORDER BY t.realizada_em DESC
    LIMIT 10;
END;
$$ LANGUAGE plpgsql;

-- função debitar
CREATE OR REPLACE FUNCTION debitar(id_cliente INT, valor_debitar INT, descricao VARCHAR)
RETURNS INT AS $$
DECLARE
    saldo_cliente INT;
    saldo_novo INT;
    limite_cliente INT;
BEGIN
    -- busca as informações atuais do cliente
SELECT saldo_inicial, limite
INTO saldo_cliente, limite_cliente
FROM Clientes
WHERE id = id_cliente
    LIMIT 1;

-- se o saldo do cliente não foi cadastrado gera erro
IF saldo_cliente IS NULL THEN 
        RAISE EXCEPTION 'Cliente não encontrado';
END IF;

    -- realiza a operação de debito em cima do saldo atual do cliente
    saldo_novo := saldo_cliente - valor_debitar;

    -- a transação não pode deixar o saldo inconsistente
    -- então verificamos se o cliente tem limite suficiente antes
    IF saldo_novo < -limite_cliente THEN
        RAISE EXCEPTION 'Cliente não tem limite suficiente para essa transação!';
END IF;

    -- atualiza o saldo do cliente
UPDATE Clientes
SET saldo_inicial = saldo_novo
WHERE id = id_cliente;

-- insere a nova transação
INSERT INTO Transacao
(id_cliente, valor, tipo, descricao, realizada_em)
VALUES (id_cliente, valor_debitar, 'd', descricao, CURRENT_TIMESTAMP);

-- retorna o novo saldo
RETURN saldo_novo;
END;
$$ LANGUAGE plpgsql;


-- Função creditar
CREATE OR REPLACE FUNCTION creditar(id_cliente INT, valor_creditar INT, descricao VARCHAR)
RETURNS INT AS $$
DECLARE
    saldo_cliente INT;
    saldo_novo INT;
BEGIN
    -- busca as informações atuais do cliente
    SELECT saldo_inicial
    INTO saldo_cliente
    FROM Clientes
    WHERE id = id_cliente
    LIMIT 1;

    -- se o saldo do cliente não foi cadastrado gera erro
    IF saldo_cliente IS NULL THEN 
        RAISE EXCEPTION 'deu errado';
    END IF;

    -- realiza a função de crédito
    saldo_novo := saldo_cliente + valor_creditar;

    -- atualiza o saldo do cliente
    UPDATE Clientes
    SET saldo_inicial = saldo_novo
    WHERE id = id_cliente;

    -- insere uma nova transação
    INSERT INTO Transacao
        (id_cliente, valor, tipo, descricao, realizada_em)
    VALUES (id_cliente, valor_creditar, 'c', descricao, CURRENT_TIMESTAMP);

    -- retorna o novo saldo
    RETURN saldo_novo;
END;
$$ LANGUAGE plpgsql;