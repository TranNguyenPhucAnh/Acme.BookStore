#!/bin/sh
echo "Current user: $(id)"
echo "Fixing permissions for /app/mynginx.store.pfx"
chmod 644 /app/mynginx.store.pfx
ls -l /app/mynginx.store.pfx
echo "Starting application"
exec dotnet Acme.BookStore.HttpApi.Host.dll