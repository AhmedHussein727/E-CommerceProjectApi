# Deployment

The API ships as a Docker image and runs anywhere that can host a container.
The Angular client is a separate repository
([E-CommerceProjectClient](https://github.com/AhmedHussein727/E-CommerceProjectClient))
and deploys as static files.

## Recommended free stack

| Piece | Service | Free tier |
| --- | --- | --- |
| API | [Render](https://render.com) web service (Docker) | Yes — sleeps after 15 min idle |
| Database | [Azure SQL Database](https://azure.microsoft.com/products/azure-sql/database) free offer | 100k vCore-seconds/month, 32 GB |
| Cache | [Upstash Redis](https://upstash.com) | 10k commands/day |
| Client | [Netlify](https://netlify.com) | Yes |

Azure SQL keeps the existing SQL Server provider, so no code or migration
changes are needed. Render's free instances cold-start: the first request after
an idle period takes roughly a minute while the container boots and migrations
run.

## Required configuration

The application reads every secret from configuration; nothing is committed.
Set these as environment variables on the host. The double underscore is how
.NET maps a flat variable onto a nested configuration key.

| Variable | Value |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Azure SQL connection string for the store database |
| `ConnectionStrings__IdentityConnection` | Azure SQL connection string for the identity database |
| `ConnectionStrings__RedisConnection` | `<host>:<port>,password=<password>,ssl=True,abortConnect=false` |
| `JWTOptions__SecretKey` | 32+ random bytes, base64. Generate with `openssl rand -base64 48` |
| `JWTOptions__Issuer` | Public URL of the API |
| `JWTOptions__Audience` | Public URL of the API |
| `Stripe__SecretKey` | Stripe secret key |
| `Stripe__EndpointSecret` | Stripe webhook signing secret |
| `Cors__AllowedOrigins__0` | Origin of the deployed client, e.g. `https://your-shop.netlify.app` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

`URLs__BaseUrl` is intentionally unset in `appsettings.json`. When absent the API builds image URLs from
the incoming request, which is normally what you want behind a proxy.

The app refuses to start if `JWTOptions__SecretKey` is missing, rather than
failing later with an obscure error.

## Deploying the API

```bash
docker build -t ecommerce-api .
docker run -p 8080:8080 --env-file .env ecommerce-api
```

On Render: create a new Web Service, point it at this repository, choose the
Docker runtime, and add the variables above. The container listens on the port
given by `ASPNETCORE_HTTP_PORTS` (8080 by default).

Schema migrations run automatically at startup for both databases. The product
catalog is seeded on first run. Demo user accounts are seeded only when
`ASPNETCORE_ENVIRONMENT=Development`, so a production instance starts with no
users — register through the API to create one.

`GET /health` returns 200 and is unauthenticated, for platform health checks.

## Deploying the client

Build with the API address baked in — Angular resolves `environment.prod.ts` at
build time, not at runtime:

1. Set `apiUrl` in `src/environments/environment.prod.ts` to
   `https://<your-api-host>/api/`.
2. `npm ci && npm run build -- --prod`
3. Publish `dist/client`.

Add that client origin to `Cors__AllowedOrigins__0` on the API, otherwise the
browser blocks every request.

## Stripe webhook

Point the webhook at `https://<your-api-host>/api/payments/webhook` and copy the
signing secret into `Stripe__EndpointSecret`. The endpoint is anonymous by
design and is authenticated by verifying the `Stripe-Signature` header.

Payments run in Stripe test mode. Use card `4242 4242 4242 4242` with any future
expiry and any CVC.

## Known limitations

- The Angular client targets Angular 11, which is past end of life. It builds
  and runs, but should be upgraded before this handles anything real.
- AutoMapper 16.2.0 logs a licensing warning at startup. Its licence permits
  development and testing; commercial production use requires a licence key.
- There is no automated test suite.
