-- DROP DATABASE DbTucano;

CREATE DATABASE IF NOT EXISTS DbTucano;
use DbTucano;

CREATE TABLE tbProduto (
ID_Produto INT PRIMARY KEY AUTO_INCREMENT,
Nome VARCHAR(150) not null,
Preco DECIMAL(6,2) not null,
Descricao VARCHAR(200) not null,
Disponibilidade BOOLEAN not null,
ID_Categoria INT not null
);

CREATE TABLE tbCategoria (
ID_Categoria INT PRIMARY KEY AUTO_INCREMENT,
Categoria VARCHAR(50)
);

CREATE TABLE tbItem_Pedido (
ID_Item INT PRIMARY KEY AUTO_INCREMENT,
NF INT,
ID_Produto INT,
Quantidade SMALLINT,
Preco DECIMAL(6,2),
FOREIGN KEY(ID_Produto) REFERENCES tbProduto (ID_Produto)
);

CREATE TABLE tbPedido (
NF INT PRIMARY KEY AUTO_INCREMENT,
DataPedido DATETIME,
ID_Usuario INT,
TipoPedido Enum('Entrega', 'Retirada'),
StatusPedido Enum('Em preparo', 'Em Entrega', 'Concluido'),
Subtotal DECIMAL(6,2),
TaxaEntrega DECIMAL(6,2),
ValorTotal DECIMAL(6,2),
CEP VARCHAR(9),
DetalhesPedido VARCHAR(150),
ID_Funcionario INT
);

CREATE TABLE tbPagamento (
ID_Pagamento INT PRIMARY KEY AUTO_INCREMENT,
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
Telefone VARCHAR(11) not null,
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
    NomeEndereco VARCHAR(150),
    Numero VARCHAR(10),
    Complemento VARCHAR(200),
    FOREIGN KEY (ID_Usuario) REFERENCES tbUsuario(ID_Usuario),
    FOREIGN KEY (CEP) REFERENCES tbEndereco(CEP)
);



CREATE TABLE tbBairro (
ID_Bairro INT PRIMARY KEY AUTO_INCREMENT,
Bairro VARCHAR(75)
);

CREATE TABLE tbCidade (
ID_Cidade INT PRIMARY KEY AUTO_INCREMENT,
Cidade VARCHAR(75)
);

CREATE TABLE tbEstado (
ID_Uf INT PRIMARY KEY AUTO_INCREMENT,
UF CHAR(2)
);

CREATE TABLE tbCartao (
ID_Cartao INT PRIMARY KEY AUTO_INCREMENT,
ID_Usuario INT,
NumeroCartao VARCHAR(100),
NomeTitular VARCHAR(75),
Apelido VARCHAR(50),
DataValidade DATE,
Bandeira VARCHAR(100),
FOREIGN KEY(ID_Usuario) REFERENCES tbUsuario (ID_Usuario)
);

CREATE TABLE tbImagem (
ID_Imagem INT PRIMARY KEY AUTO_INCREMENT,
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

INSERT INTO tbUsuario (Nome, Email, Senha, CPF, Telefone, DataNasc, Sexo)
VALUES ('Administrador', 'admin@bicotucano.com', '123456', '00000000000', 11999999999, '2000-01-01', 'M');

INSERT INTO tbFuncionario (ID_Funcionario, DataAdmissao, NivelAcesso, Cargo)
VALUES (LAST_INSERT_ID(), CURDATE(), 'Administrador', 'Gerente');

INSERT INTO tbCategoria (Categoria)
VALUES ("Entrada"),
	   ("Prato completo"),
       ("Lanche"),
	   ("Sobremesa"),
       ("Bebida");

INSERT INTO tbProduto (Nome, Preco, Descricao, Disponibilidade, ID_Categoria)
VALUES
-- ENTRADAS (ID_Categoria = 1)
('Batata Frita', 18.90, 'Batatas fritas em óleo vegetal, temperadas com sal e ervas, acompanhadas de maionese de abacate ou castanha.', TRUE, 1),
('Mandioca Frita', 19.90, 'Mandioca frita com sal grosso e salsinha, acompanhada de maionese de castanha ou molho artesanal de pimenta.', TRUE, 1),
('Creme de Tofu', 17.90, 'Tofu cremoso com azeite, alho assado, limão e ervas, acompanhado de torradas artesanais.', TRUE, 1),
('Legumes Salteados', 16.90, 'Abobrinha, cenoura, brócolis e pimentão salteados com azeite, alho, shoyu, gengibre e gergelim.', TRUE, 1),

-- LANCHES (ID_Categoria = 3)
('Hambúrguer de Jaca', 29.90, 'Hambúrguer de Jaca verde desfiada e temperada, servida com alface, tomate e maionese de castanha.', TRUE, 3),
('Hambúrguer de Soja', 28.90, 'Hambúrguer de proteína de soja com alface, tomate, cebola roxa e molho de mostarda com agave.', TRUE, 3),
('Hambúrguer de Grão-de-Bico', 30.90, 'Hambúrguer de grão-de-bico com cenoura, coentro e cominho, acompanhado de maionese de abacate.', TRUE, 3),
('Hambúrguer com Maionese de Abacate', 32.90, 'Hambúrguer de soja com maionese de abacate, alface americana, tomate e cebola roxa caramelizada.', TRUE, 3),
('Sanduíche de Tofu Grelhado', 27.90, 'Tofu marinado no shoyu, alho e gengibre, servido no pão integral com rúcula, tomate seco e maionese de castanha.', TRUE, 3),

-- PRATOS COMPLETOS (ID_Categoria = 2)
('Penne ao Pesto', 42.90, 'Penne com pesto vegano de manjericão, castanha, azeite, alho e levedura nutricional, acompanhado de tomate-cereja.', TRUE, 2),
('Moqueca de Berinjela', 48.90, 'Berinjela com leite de coco, pimentões, tomate, cebola, coentro e azeite de dendê, acompanhada de arroz branco.', TRUE, 2),
('Batata-Doce Recheada', 39.90, 'Batata-doce assada recheada com creme de tofu, proteína de soja ao molho de tomate ou legumes salteados com quinoa.', TRUE, 2),
('Refogado de Tofu com Legumes', 44.90, 'Tofu com brócolis, cenoura, ervilha e milho, ao molho de shoyu e gengibre, acompanhado de arroz integral.', TRUE, 2),
('Nhoque Vegano', 45.90, 'Nhoque de batata sem ovos, servido com molho de tomate e manjericão ou pesto.', TRUE, 2),
('Lasanha de Berinjela', 52.90, 'Camadas de berinjela grelhada com bolonhesa de soja, molho de tomate e creme de tofu.', TRUE, 2),
('Fricassê Vegano', 49.90, 'Proteína de soja desfiada com creme de milho vegano e batata palha, preparado no forno.', TRUE, 2),
('Ratatouille', 46.90, 'Abobrinha, berinjela, tomate e pimentão em camadas, assados com azeite, alho, tomilho e manjericão.', TRUE, 2),

-- BEBIDAS (ID_Categoria = 5)
('Suco de Abacaxi', 10.90, 'Suco natural de abacaxi.', TRUE, 5),
('Suco de Goiaba', 10.90, 'Suco natural de goiaba.', TRUE, 5),
('Suco de Laranja', 10.90, 'Suco natural de laranja.', TRUE, 5),
('Suco de Manga', 10.90, 'Suco natural de manga.', TRUE, 5),
('Suco de Melancia', 10.90, 'Suco natural de melancia.', TRUE, 5),
('Limonada', 10.90, 'Suco natural de limão', TRUE, 5),
('Coca-Cola', 7.90, 'Refrigerante em lata de 350 ml.', TRUE, 5),
('Guaraná Antarctica', 7.90, 'Refrigerante em lata de 350 ml.', TRUE, 5),
('Sprite', 7.90, 'Refrigerante em lata de 350 ml.', TRUE, 5),
('Chá de Hibisco', 11.90, 'Chá gelado de hibisco.', TRUE, 5),
('Chá de Camomila', 11.90, 'Chá gelado de camomila.', TRUE, 5),
('Vitamina de Banana com Cacau', 14.90, 'Vitamina vegana de banana com cacau.', TRUE, 5),
('Vitamina de Morango', 14.90, 'Vitamina vegana de morango.', TRUE, 5),
('Vitamina de Açaí com Banana', 14.90, 'Vitamina vegana de açaí com banana.', TRUE, 5),
('Vitamina de Abacate', 14.90, 'Vitamina vegana de abacate.', TRUE, 5),
('Água de Coco', 9.90, 'Água de coco natural e gelada.', TRUE, 5),

-- SOBREMESAS (ID_Categoria = 4)
('Bolo de Cenoura', 14.90, 'Bolo vegano de cenoura com cobertura de chocolate.', TRUE, 4),
('Bolo de Chocolate', 14.90, 'Bolo vegano de chocolate.', TRUE, 4),
('Bolo de Banana', 14.90, 'Bolo vegano de banana com canela.', TRUE, 4),
('Mousse de Maracujá', 16.90, 'Mousse vegana de castanha com polpa de maracujá, agave e óleo de coco.', TRUE, 4),
('Brigadeiro de Banana', 12.90, 'Brigadeiro vegano de banana madura, cacau e óleo de coco, com granulado vegano. Porção com 4 unidades.', TRUE, 4),
('Sorvete de Banana', 13.90, 'Sorvete vegano de banana congelada, disponível na versão natural ou com morango.', TRUE, 4),
('Doce de Abóbora', 11.90, 'Doce de abóbora cabotiá com cravo, canela e agave, servido em pote individual.', TRUE, 4);

INSERT INTO tbImagem (ID_Produto, NomeImagem)
VALUES
-- ENTRADAS
(1, 'batatafrita.jpg'),
(2, 'mandiocafrita.jpg'),
(3, 'cremedetofu.jpg'),
(4, 'legumessalteados.jpg'),

-- LANCHES
(5, 'burguerjaca.jpg'),
(6, 'burguersoja.jpg'),
(7, 'burguergraodebico.jpg'),
(8, 'burguerabacate.jpg'),
(9, 'sanduichetofu.jpg'),

-- PRATOS COMPLETOS
(10, 'penneaopesto.jpg'),
(11, 'muquecaberinjela.jpg'),
(12, 'batatarecheada.jpg'),
(13, 'refogadotofu.jpg'),
(14, 'nhoquepesto.jpg'),
(15, 'lasanhaberinjela.jpg'),
(16, 'fricasse.jpg'),
(17, 'ratatouille.jpg'),

-- BEBIDAS
(18, 'sucoabacaxi.jpg'),
(19, 'sucogoiaba.jpg'),
(20, 'sucolaranja.jpg'),
(21, 'sucomanga.jpg'),
(22, 'sucomelancia.jpg'),
(23, 'limonada.jpg'),
(24, 'cocacola.jpg'),
(25, 'guarana.jpg'),
(26, 'sprite.jpg'),
(27, 'chahibisco.jpg'),
(28, 'chacamomila.jpg'),
(29, 'vitaminabanana.jpg'),
(30, 'vitaminamorango.jpg'),
(31, 'vitaminaacai.jpg'),
(32, 'vitaminaabacate.jpg'),
(33, 'aguadecoco.jpg'),

-- SOBREMESAS
(34, 'bolocenoura.jpg'),
(35, 'bolochocolate.jpg'),
(36, 'bolobanana.jpg'),
(37, 'moussemaracuja.jpg'),
(38, 'brigadeirobanana.jpg'),
(39, 'sorvetebanana.jpg'),
(40, 'doceabobora.jpg');