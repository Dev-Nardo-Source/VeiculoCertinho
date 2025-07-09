FROM mcr.microsoft.com/playwright:v1.53.2-noble

WORKDIR /app

COPY package*.json ./
RUN npm ci --only=production

COPY . .

EXPOSE 3000

CMD ["node", "index.js"]