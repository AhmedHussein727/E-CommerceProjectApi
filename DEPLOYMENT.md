# Deployment

The API ships as a Docker image and runs anywhere that can host a container.
The Angular client is a separate repository
([E-CommerceProjectClient](https://github.com/AhmedHussein727/E-CommerceProjectClient))
and deploys as static files.

## Recommended free stack

| Piece | Service | Free tier |
| --- | --- | --- |
| API | [MonsterASP.NET](https://www.monsterasp.net) free plan (IIS, EU datacenter) | 5 GB disk, 256 MB RAM, free subdomain, no card |
| Database | MonsterASP MSSQL | One database, 1 GB. Both DbContexts share it |
| Cache | [Upstash Redis](https://upstash.com) | 10k commands/day |
| Client | [Netlify](https://netlify.com) | Yes |

No payment method is involved anywhere in this stack.

The free plan allows a single database, so `DefaultConnection` and
`IdentityConnection` point at the same one. The identity context writes to
`__EFMigrationsHistory_Identity` so the two do not overwrite each other's
migration records. Their tables do not otherwise collide.

The EU datacenter is in Germany, which is also where the cache should live.

## Required configuration

The application reads every secret from configuration; nothing is committed.
Set these as environment variables on the host. The double underscore is how
.NET maps a flat variable onto a nested configuration key.

| Variable | Value |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string, e.g. `Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True` |
| `ConnectionStrings__IdentityConnection` | Same value as above on a single-database plan |
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

On MonsterASP (IIS), publish and upload the output over FTP:

```bash
dotnet publish ECommerce.API/ECommerce.Web.csproj -c Release -o ./publish
```

Upload the contents of `./publish` to the site root. `web.config` is generated
by publish and wires up the ASP.NET Core module. Set the configuration values
by uploading an `appsettings.Production.json` alongside it — that filename is
git-ignored, so the secrets never reach the repository.

For any container platform instead:

```bash
docker build -t ecommerce-api .
docker run -p 8080:8080 --env-file .env ecommerce-api
```

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
- `Order.OrderDate` is written as UTC. Keep it that way if the provider ever
  changes again; PostgreSQL rejects a non-UTC `DateTimeOffset` outright.
- The Dockerfile still builds and runs. It is unused on IIS shared hosting but
  keeps the app deployable to any container platform.
