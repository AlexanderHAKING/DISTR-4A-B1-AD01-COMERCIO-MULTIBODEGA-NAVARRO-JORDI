using System.Net;

namespace Pedidos.Api.Data;

public class ConsultaProducto
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    public ConsultaProducto(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<bool?> Existe(int idProducto)
    {
        var url = _config["Servicios:CatalogoUrl"];
        try
        {
            var respuesta = await _http.CreateClient().GetAsync($"{url}/api/Productos/{idProducto}");
            if (respuesta.StatusCode == HttpStatusCode.NotFound) return false;
            if (respuesta.IsSuccessStatusCode) return true;
            return null;
        }
        catch (HttpRequestException) { return null; }
        catch (TaskCanceledException) { return null; }
    }
}
