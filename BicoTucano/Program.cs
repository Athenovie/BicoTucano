using BicoTucano.Libraries.Login;
using BicoTucano.Repository;
using BicoTucano.Repository.Contract;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//Adicionar a Interface como um serviço 
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();


// Corrigir problema com TEMPDATA
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // Definir um tempo para duracao.
    options.IdleTimeout = TimeSpan.FromSeconds(60);
    options.Cookie.HttpOnly = true;

    // Mostrar para o navegador que o cookie é essencial
    options.Cookie.IsEssential = true;
});

builder.Services.AddMvc().AddSessionStateTempDataProvider();
builder.Services.AddScoped<BicoTucano.Libraries.Sessao.Sessao>();
builder.Services.AddScoped<LoginCliente>();
builder.Services.AddScoped<LoginFuncionario>();

//Adicionado para manipular a Sessão
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}



app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
