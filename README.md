# D2StatCollector
Initial prototype. Actually, the second prototype.

Todos:
- Logging
- Error handling
- Improve Thread handling

## What it does
This application collects data from the Destiny 2 API and stores it in a database.
It does this repeatedly for every user in one or multiple Clans.
Per default, it collects every 10 minutes, but you can adapt this setting.
**I suggest that you do not lower this below 5 minutes**.

## What it looks like
The docker-compose file starts a Grafana instance, which you can use to visualize the data.
Very early screenshot:
![img.png](img.png)


## How to use
Modify the environment variables in the `.env-*` files to your needs.
Then you can use `docker compose up` to start the application (s).

Build the image with `docker build --target final -f D2StatCollector.Server/Dockerfile -t d2stat-collector:dev .`.

## Contribution
Feel free to contribute to this project. Just open a pull request.
