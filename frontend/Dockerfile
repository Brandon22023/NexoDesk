ARG NODE_IMAGE

FROM ${NODE_IMAGE}

ARG FRONTEND_CONTAINER_PORT

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

EXPOSE ${FRONTEND_CONTAINER_PORT}

CMD ["npm", "run", "dev"]

