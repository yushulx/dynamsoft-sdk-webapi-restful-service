const express = require('express');
const path = require('path');
const fs = require('fs');
const app = express();
const http = require('http');
const server = http.createServer(app);
const bodyParser = require('body-parser');

app.use(express.static('public'));
app.use('/node_modules', express.static(__dirname + '/node_modules'));
app.use(bodyParser.json({ limit: '50mb' }));
app.use(bodyParser.urlencoded({ limit: '50mb', extended: true }));

// Start the server
const port = process.env.PORT || 3000;

server.listen(port, '0.0.0.0', () => {
    host = server.address().address;
    console.log(`Server running at http://0.0.0.0:${port}/`);
});
