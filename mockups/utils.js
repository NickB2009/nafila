// Utility functions for the mockup pages

// Add the developer mockups navigation bar to the page
function addMockupsNavBar() {
    // Create the navigation bar
    const mockLinksBar = document.createElement('div');
    mockLinksBar.className = 'mock-links-bar';
    mockLinksBar.innerHTML = `
        <button class="mock-links-toggle">
            <i class="fas fa-code"></i>
        </button>
        <div class="mock-links-content">
            <div class="mock-links-title">Acessar Mockups (Desenvolvedor)</div>
            <div class="mock-links">
                <a href="telaInicial.html" class="mock-link"><i class="fas fa-home"></i> Tela Inicial</a>
                <a href="checkIn.html" class="mock-link"><i class="fas fa-clipboard-list"></i> Check-in</a>
                <a href="telaAutoatendimento.html" class="mock-link"><i class="fas fa-tablet-alt"></i> Autoatendimento</a>
                <a href="painelFila.html" class="mock-link"><i class="fas fa-users"></i> Painel da Fila</a>
                <a href="loginBarbeiro.html" class="mock-link"><i class="fas fa-user-lock"></i> Login Barbeiro</a>
                <a href="painelAdmin.html" class="mock-link"><i class="fas fa-cogs"></i> Painel Admin</a>
            </div>
        </div>
    `;
    
    // Add the navigation bar styles
    const style = document.createElement('style');
    style.textContent = `
        .mock-links-bar {
            position: fixed;
            bottom: 1rem;
            right: 1rem;
            z-index: 1000;
            display: flex;
            flex-direction: column;
            align-items: flex-end;
        }
        
        .mock-links-toggle {
            width: 40px;
            height: 40px;
            background-color: rgba(43, 43, 43, 0.95);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
            color: white;
            font-size: 1.2rem;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
            transition: all 0.3s ease;
            border: none;
            padding: 0;
        }

        .mock-links-toggle:hover {
            transform: scale(1.1);
            background-color: rgba(43, 43, 43, 1);
        }

        .mock-links-content {
            display: none;
            position: absolute;
            bottom: calc(100% + 1rem);
            right: 0;
            background-color: rgba(43, 43, 43, 0.95);
            padding: 1rem;
            border-radius: 12px;
            color: white;
            font-size: 0.8rem;
            backdrop-filter: blur(10px);
            box-shadow: 0 4px 20px rgba(0,0,0,0.2);
            min-width: 200px;
            max-width: 300px;
            transform-origin: bottom right;
        }

        .mock-links-content.active {
            display: block;
            animation: slideUp 0.3s ease;
        }

        @keyframes slideUp {
            from {
                opacity: 0;
                transform: translateY(10px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        .mock-links-title {
            margin-bottom: 0.8rem;
            font-weight: 600;
            color: #ffc107;
            font-size: 0.9rem;
        }
        
        .mock-links {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }
        
        .mock-link {
            color: white;
            text-decoration: none;
            background-color: rgba(255,255,255,0.1);
            padding: 0.5rem 0.8rem;
            border-radius: 6px;
            transition: all 0.3s ease;
            border: 1px solid rgba(255,255,255,0.2);
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-size: 0.85rem;
        }
        
        .mock-link:hover {
            background-color: rgba(255,255,255,0.2);
            transform: translateX(-5px);
        }
    `;
    
    // Add elements to the page
    document.head.appendChild(style);
    document.body.appendChild(mockLinksBar);

    // Add click handler for toggle button
    const toggleButton = mockLinksBar.querySelector('.mock-links-toggle');
    const content = mockLinksBar.querySelector('.mock-links-content');
    
    toggleButton.addEventListener('click', function(e) {
        e.stopPropagation();
        content.classList.toggle('active');
    });

    // Close menu when clicking outside
    document.addEventListener('click', function(e) {
        if (!mockLinksBar.contains(e.target)) {
            content.classList.remove('active');
        }
    });
}

// Call this function in other pages to add the navigation bar
if (typeof window !== 'undefined') {
    window.addEventListener('DOMContentLoaded', addMockupsNavBar);
}

// Utility function to check authentication
function checkBarberAuth() {
    // In a real app, this would check with the server
    // For mockup purposes, just check if we have a URL parameter
    const urlParams = new URLSearchParams(window.location.search);
    const barberId = urlParams.get('barbeiro');
    
    if (!barberId) {
        // Redirect to login page if not authenticated
        window.location.href = '/loginBarbeiro.html';
        return false;
    }
    
    return true;
}

// Export for use in other scripts
if (typeof module !== 'undefined') {
    module.exports = {
        addMockupsNavBar,
        checkBarberAuth
    };
} 