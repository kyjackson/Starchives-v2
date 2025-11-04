FROM debian:bookworm-slim

ARG BUILD_VERSION
ARG BUILD_SHA
ARG BUILD_TIME

LABEL org.opencontainers.image.title="Starchives"
LABEL org.opencontainers.image.version=$BUILD_VERSION
LABEL org.opencontainers.image.revision=$BUILD_SHA
LABEL org.opencontainers.image.created=$BUILD_TIME

WORKDIR /app
COPY app/ ./

ENV APP_VERSION=$BUILD_VERSION \
    APP_SHA=$BUILD_SHA \
    APP_BUILD_TIME=$BUILD_TIME \
    ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080
CMD ["./Starchives"]
