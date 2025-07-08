namespace LalaHealthCare.App.Models;

public class WebViewCredentials
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Url { get; set; } = "https://lalahealthcareqa-dre0cwdkbafscmc5.eastus2-01.azurewebsites.net/";

    public string GetAutoLoginScript()
    {
        // JavaScript para auto-completar el formulario de login
        return $@"
                (function() {{
                    // Esperar a que la página cargue completamente
                    function tryLogin() {{
                        // Buscar campos de username/email
                        var usernameField = document.querySelector('input[type=""text""][name*=""user"" i]') || 
                                          document.querySelector('input[type=""email""]') ||
                                          document.querySelector('input[id*=""user"" i]') ||
                                          document.querySelector('input[name*=""email"" i]');
                        
                        // Buscar campo de password
                        var passwordField = document.querySelector('input[type=""password""]') ||
                                          document.querySelector('input[name*=""pass"" i]') ||
                                          document.querySelector('input[id*=""pass"" i]');
                        
                        if (usernameField && passwordField) {{
                            usernameField.value = '{Username}';
                            passwordField.value = '{Password}';
                            
                            // Disparar eventos para que el formulario detecte los cambios
                            usernameField.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            passwordField.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            usernameField.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            passwordField.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            
                            // Opcional: intentar hacer click en el botón de login
                            // var loginButton = document.querySelector('button[type=""submit""]') || 
                            //                  document.querySelector('input[type=""submit""]') ||
                            //                  document.querySelector('button[id*=""login"" i]');
                            // if (loginButton) {{
                            //     setTimeout(() => loginButton.click(), 500);
                            // }}
                            
                            return true;
                        }}
                        return false;
                    }}
                    
                    // Intentar varias veces por si la página carga dinámicamente
                    let attempts = 0;
                    const maxAttempts = 10;
                    const interval = setInterval(() => {{
                        if (tryLogin() || attempts >= maxAttempts) {{
                            clearInterval(interval);
                        }}
                        attempts++;
                    }}, 1000);
                }})();
            ";
    }
}