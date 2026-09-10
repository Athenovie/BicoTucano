 -- Drop database DbTucano

Create database DbTucano;
use DbTucano;

CREATE TABLE tbProduto (
ID_Produto INT PRIMARY KEY,
Nome VARCHAR(150) not null,
Preco DECIMAL(6,2) not null,
Descricao VARCHAR(200) not null,
Disponibilidade BOOLEAN not null,
ID_Categoria INT not null
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
ID_Usuario INT PRIMARY KEY auto_increment,
Nome VARCHAR(100) not null,
Email VARCHAR(100) unique not null,
Senha VARCHAR(150)not null,
CPF VARCHAR(11) unique not null,
Telefone DECIMAL(11) not null,
DataNasc DATE not null,
Sexo CHAR(1) not null
);

CREATE TABLE tbEndereco (
CEP VARCHAR(9) PRIMARY KEY,
Bairro INT,
Cidade INT,
Estado INT,
Logradouro VARCHAR(75)
);

CREATE TABLE tbUsuarioEndereco (
    ID_UsuarioEndereco INT PRIMARY KEY AUTO_INCREMENT,
    ID_Usuario INT,
    CEP VARCHAR(9),
    TipoEndereco VARCHAR(150),
    Numero VARCHAR(10),
    Complemento VARCHAR(200),
    FOREIGN KEY (ID_Usuario) REFERENCES tbUsuario(ID_Usuario),
    FOREIGN KEY (CEP) REFERENCES tbEndereco(CEP)
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
ALTER TABLE tbEndereco ADD FOREIGN KEY(Bairro) REFERENCES tbBairro (ID_Bairro);
ALTER TABLE tbEndereco ADD FOREIGN KEY(Cidade) REFERENCES tbCidade (ID_Cidade);
ALTER TABLE tbEndereco ADD FOREIGN KEY(Estado) REFERENCES tbEstado (ID_Uf);



