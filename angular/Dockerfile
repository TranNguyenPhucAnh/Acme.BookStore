FROM node:18-alpine AS build

WORKDIR /app
COPY package*.json /app/

RUN yarn
COPY . /app

RUN yarn build:prod

FROM nginx:alpine

COPY nginx.conf /etc/nginx/conf.d/default.conf
# COPY dynamic-env.json /usr/share/nginx/html
COPY --from=build /app/dist/BookStore /usr/share/nginx/html