const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const morgan = require('morgan');
require('dotenv').config();

const app = express();

// Middlewares
app.use(helmet());
app.use(cors());
app.use(morgan('dev'));
app.use(express.json());

// Importar serviços
const scrapingService = require('./src/services/scrapingService');

// Rotas
app.get('/', (req, res) => {
  res.json({ 
    message: 'VeiculoCertinho API está funcionando!',
    endpoints: {
      health: '/health',
      consultar: '/api/veiculo/:placa'
    }
  });
});

app.get('/health', (req, res) => {
  res.json({ 
    status: 'OK',
    service: 'VeiculoCertinho',
    timestamp: new Date()
  });
});

// Rota de consulta de veículo
app.get('/api/veiculo/:placa', async (req, res) => {
  try {
    const { placa } = req.params;
    const resultado = await scrapingService.consultarVeiculo(placa);
    res.json(resultado);
  } catch (error) {
    res.status(500).json({ 
      error: 'Erro ao consultar veículo',
      message: error.message 
    });
  }
});

// Porta
const PORT = process.env.PORT || 3000;

app.listen(PORT, () => {
  console.log(`Servidor rodando na porta ${PORT}`);
  console.log(`Acesse: http://localhost:${PORT}`);
});