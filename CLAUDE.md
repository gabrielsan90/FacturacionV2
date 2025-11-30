- siempre usar agentes y basarse en el archivo especificacion_sistema.md
- usando agents: en el codigo javascript NO usar la llamada directa al api, en el ajax call hacer el llamado directo al ?handler en el code behind y ahi hacer el llamado al api por medio de un httpclientfactory

la forma correcta de usar IHttpClientFactory para hacer la llamada al API es asi: 
private readonly IHttpClientFactory _http;

var client = _http.CreateClient("Api");
if (Request.Cookies.TryGetValue("jwtAdmin", out var jwt))
    client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);
var resp = await client.GetAsync("/api/direccion");

 if (resp.IsSuccessStatusCode)
 {
}