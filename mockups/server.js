const express = require('express');
const path = require('path');
const app = express();

// Serve static files from mockups directory
app.use(express.static(__dirname));

// Serve the mockup HTML files
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'telaInicial.html'));
});

app.get('/self-service', (req, res) => {
    res.sendFile(path.join(__dirname, 'telaAutoatendimento.html'));
});

app.get('/login', (req, res) => {
    res.sendFile(path.join(__dirname, 'login.html'));
});

app.get('/checkin', (req, res) => {
    res.sendFile(path.join(__dirname, 'checkIn.html'));
});

app.get('/loginBarbeiro', (req, res) => {
    res.sendFile(path.join(__dirname, 'loginBarbeiro.html'));
});

app.get('/painelAdmin', (req, res) => {
    res.sendFile(path.join(__dirname, 'painelAdmin.html'));
});

app.get('/painelBarbeiro', (req, res) => {
    res.sendFile(path.join(__dirname, 'painelBarbeiro.html'));
});

app.get('/painelFila', (req, res) => {
    res.sendFile(path.join(__dirname, 'painelFila.html'));
});

app.get('/contaUsuario', (req, res) => {
    res.sendFile(path.join(__dirname, 'contaUsuario.html'));
});

const PORT = process.env.PORT || 1000;
app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
    console.log(`http://localhost:${PORT}`);
}); 