// webview-helper.js
export function injectCredentials(username, password) {
    try {
        const iframe = document.getElementById('webFrame');
        if (!iframe || !iframe.contentWindow) {
            console.warn('No se pudo acceder al iframe');
            return;
        }

        // Intentar inyectar credenciales en campos comunes
        const iframeDoc = iframe.contentDocument || iframe.contentWindow.document;

        if (iframeDoc) {
            // Buscar campos de usuario comunes
            const usernameFields = iframeDoc.querySelectorAll(
                'input[type="text"][name*="user"], input[type="text"][name*="email"], ' +
                'input[type="email"], input[id*="user"], input[id*="login"], ' +
                'input[placeholder*="user"], input[placeholder*="email"]'
            );

            // Buscar campos de contraseña
            const passwordFields = iframeDoc.querySelectorAll(
                'input[type="password"], input[name*="pass"], input[id*="pass"]'
            );

            // Inyectar username
            if (usernameFields.length > 0) {
                usernameFields[0].value = username;
                usernameFields[0].dispatchEvent(new Event('input', { bubbles: true }));
                usernameFields[0].dispatchEvent(new Event('change', { bubbles: true }));
            }

            // Inyectar password
            if (passwordFields.length > 0) {
                passwordFields[0].value = password;
                passwordFields[0].dispatchEvent(new Event('input', { bubbles: true }));
                passwordFields[0].dispatchEvent(new Event('change', { bubbles: true }));
            }

            console.log(`Credenciales inyectadas: Usuario: ${usernameFields.length > 0}, Password: ${passwordFields.length > 0}`);

            // Opcional: Auto-submit si hay un botón de login
            setTimeout(() => {
                const loginButtons = iframeDoc.querySelectorAll(
                    'button[type="submit"], input[type="submit"], ' +
                    'button[id*="login"], button[id*="submit"], ' +
                    'button:contains("Login"), button:contains("Sign in")'
                );

                if (loginButtons.length > 0) {
                    console.log('Intentando auto-login...');
                    loginButtons[0].click();
                }
            }, 500);
        }
    } catch (error) {
        console.error('Error inyectando credenciales:', error);
        // Los errores de cross-origin son normales aquí
    }
}

export function refreshFrame() {
    try {
        const iframe = document.getElementById('webFrame');
        if (iframe) {
            iframe.src = iframe.src;
        }
    } catch (error) {
        console.error('Error refrescando frame:', error);
    }
}

export function getFrameUrl() {
    try {
        const iframe = document.getElementById('webFrame');
        return iframe ? iframe.src : '';
    } catch (error) {
        console.error('Error obteniendo URL del frame:', error);
        return '';
    }
}

// Función para manejar mensajes del iframe (si la página web lo soporta)
export function setupMessageListener(dotNetRef) {
    window.addEventListener('message', function (event) {
        // Verificar origen si es necesario
        if (event.data && event.data.type === 'lala-healthcare') {
            dotNetRef.invokeMethodAsync('HandleWebMessage', event.data);
        }
    });
}