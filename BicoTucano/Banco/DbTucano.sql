 -- Drop database DbTucano

Create database DbTucano;
use DbTucano;

CREATE TABLE tbProduto (
ID_Produto INT PRIMARY KEY,
Nome VARCHAR(150),
Preco DECIMAL(6,2),
Descricao VARCHAR(200),
Disponibilidade BOOLEAN,
ID_Categoria INT
);

CREATE TABLE tbCategoria (
ID_Categoria INT PRIMARY KEY,
Categoria VARCHAR(50),
Descricao VARCHAR(100)
);

CREATE TABLE tbItem_Pedido (
ID_Item INT PRIMARY KEY,
NF INT,
ID_Produto INT,
Quantidade SMALLINT,
Preco DECIMAL(6,2),
FOREIGN KEY(ID_Produto) REFERENCES tbProduto (ID_Produto)
);

CREATE TABLE tbPedido (
NF INT PRIMARY KEY,
DataPedido DATETIME,
ID_Usuario INT,
TipoPedido Enum("Entrega", "Retirada"),
StatusPedido Enum('Concluido','Em preparo', 'Em Entrega', 'Finalizado'),
Subtotal DECIMAL(6,2),
TaxaEntrega DECIMAL(6,2),
ValorTotal DECIMAL(6,2),
CEP VARCHAR(9),
DetalhesPedido VARCHAR(150),
ID_Funcionario INT
);

CREATE TABLE tbPagamento (
ID_Pagamento INT PRIMARY KEY,
NF INT,
FormaPagamento Enum('Pix', 'Cartão Débito', 'Cartão Crédito'),
ChavePix VARCHAR(150),
ID_Cartao INT,
ValorPago DECIMAL(6,2),
DataPagamento DATETIME,
StatusPagamento Enum('Pago','Não Pago'),
FOREIGN KEY(NF) REFERENCES tbPedido (NF)
);

CREATE TABLE tbUsuario (
ID_Usuario INT PRIMARY KEY,
CepUsuario VARCHAR(9)  null,
Nome VARCHAR(100),
Email VARCHAR(100),
Senha VARCHAR(150),
CPF VARCHAR(11),
Telefone DECIMAL(11),
DataNasc DATE,
Sexo CHAR(1),
Numero VARCHAR(10),
Complemento VARCHAR(200)
);

CREATE TABLE tbEndereco (
CEP VARCHAR(9) PRIMARY KEY,
Bairro INT,
Cidade INT,
Estado INT,
Logradouro VARCHAR(75)
);

CREATE TABLE tbBairro (
ID_Bairro INT PRIMARY KEY,
Bairro VARCHAR(75)
);

CREATE TABLE tbCidade (
ID_Cidade INT PRIMARY KEY,
Cidade VARCHAR(75)
);

CREATE TABLE tbEstado (
ID_Uf INT PRIMARY KEY,
UF CHAR(2)
);

CREATE TABLE tbCartao (
ID_Cartao INT PRIMARY KEY,
ID_Usuario INT,
NumeroCartao VARCHAR(100),
NomeTitular VARCHAR(75),
Apelido VARCHAR(50),
DataValidade DATE,
Bandeira VARCHAR(100),
FOREIGN KEY(ID_Usuario) REFERENCES tbUsuario (ID_Usuario)
);

CREATE TABLE tbImagem (
ID_Imagem INT PRIMARY KEY,
ID_Produto INT,
NomeImagem VARCHAR(100),
FOREIGN KEY(ID_Produto) REFERENCES tbProduto (ID_Produto)
);

CREATE TABLE tbCliente (
ID_Cliente INT PRIMARY KEY,
DataCadastro DATE,
Situacao CHAR(1),
FOREIGN KEY(ID_Cliente) REFERENCES tbUsuario (ID_Usuario)
);

CREATE TABLE tbFuncionario (
ID_Funcionario INT PRIMARY KEY,
DataAdmissao DATE,
DataDemissao DATE,
NivelAcesso ENUM ('Comum', 'Administrador'),
Cargo VARCHAR(75),
FOREIGN KEY(ID_Funcionario) REFERENCES tbUsuario (ID_Usuario)
);

ALTER TABLE tbProduto ADD FOREIGN KEY(ID_Categoria) REFERENCES tbCategoria (ID_Categoria);
ALTER TABLE tbItem_Pedido ADD FOREIGN KEY(NF) REFERENCES tbPedido (NF);
ALTER TABLE tbPedido ADD FOREIGN KEY(ID_Usuario) REFERENCES tbUsuario (ID_Usuario);
ALTER TABLE tbPedido ADD FOREIGN KEY(CEP) REFERENCES tbEndereco (CEP);
ALTER TABLE tbPedido ADD FOREIGN KEY(ID_Funcionario) REFERENCES tbFuncionario (ID_Funcionario);
ALTER TABLE tbPagamento ADD FOREIGN KEY(ID_Cartao) REFERENCES tbCartao (ID_Cartao);
ALTER TABLE tbUsuario ADD FOREIGN KEY(CepUsuario) REFERENCES tbEndereco (CEP);
ALTER TABLE tbEndereco ADD FOREIGN KEY(Bairro) REFERENCES tbBairro (ID_Bairro);
ALTER TABLE tbEndereco ADD FOREIGN KEY(Cidade) REFERENCES tbCidade (ID_Cidade);
ALTER TABLE tbEndereco ADD FOREIGN KEY(Estado) REFERENCES tbEstado (ID_Uf);


INSERT INTO tbUsuario
(ID_Usuario, CepUsuario, Nome, Email, Senha, CPF, Telefone, DataNasc, Sexo, Numero, Complemento)
VALUES
(1, NULL, 'João Silva', 'joao@email.com', '123456', '11111111111', 11987654321, '2000-05-15', 'M', '120', NULL),
(2, NULL, 'Maria Santos', 'maria@email.com', '123456', '22222222222', 11987654322, '2001-08-20', 'F', '250', 'Apto 12'),
(3, NULL, 'Carlos Oliveira', 'carlos@email.com', '123456', '33333333333', 11987654323, '1999-03-10', 'M', '500', NULL),
(4, NULL, 'Ana Costa', 'ana@email.com', '123456', '44444444444', 11987654324, '2002-11-25', 'F', '850', 'Apto 45'),
(5, NULL, 'Pedro Souza', 'pedro@email.com', '123456', '55555555555', 11987654325, '1998-07-30', 'M', '100', NULL),
(6, NULL, 'Lucas Almeida', 'lucas@email.com', '123456', '66666666666', 11987654326, '2000-01-12', 'M', '75', NULL),
(7, NULL, 'Rafael Lima', 'rafael@email.com', '123456', '77777777777', 21987654327, '1997-09-18', 'M', '30', NULL),
(8, NULL, 'Beatriz Rocha', 'beatriz@email.com', '123456', '88888888888', 31987654328, '2001-12-05', 'F', '420', 'Sala 3');

INSERT INTO tbCliente
(ID_Cliente, DataCadastro, Situacao)
VALUES
(1, '2026-01-10', 'A'),
(2, '2026-01-15', 'A'),
(3, '2026-02-01', 'A'),
(4, '2026-02-10', 'A'),
(5, '2026-03-05', 'A'),
(6, '2026-03-20', 'A');

INSERT INTO tbFuncionario
(ID_Funcionario, DataAdmissao, DataDemissao, NivelAcesso, Cargo)
VALUES
(7, '2025-01-10', NULL, 'Administrador', 'Gerente'),
(8, '2025-06-15', NULL, 'Comum', 'Atendente');