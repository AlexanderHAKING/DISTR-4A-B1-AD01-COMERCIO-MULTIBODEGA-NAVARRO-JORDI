using System.Net;

namespace Inventario.Api.Data;

public class ConsultaCatalogo
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public ConsultaCatalogo(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<bool?> ExisteProducto(int idProducto)
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
