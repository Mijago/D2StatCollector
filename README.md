# D2StatCollector
A continuous stat collector for Destiny 2.



## What it does
This application collects data from the Destiny 2 API and stores it in a database.
It does this repeatedly for every user in **one or multiple Clans**.

Per default, it collects every 10 minutes, but you can adapt this setting.
I suggest that you do not lower this below 5 minutes.

## What it looks like
The docker-compose file starts a Grafana instance, which you can use to visualize the data.
You can use the collected data to make dashboards like this one. The json for this dashboard can be found [here](doc/example-dashboard.json).

<img src="doc/example-01.png" style="max-width:200px;">
<img src="doc/example-02.png" style="max-width:200px;">
<img src="doc/example-03.png" style="max-width:200px;">

## How to use
1. Modify the environment variables in the `.env-*` files to your needs.
   2. **DEFINITELY** change the passwords to something secure.
2. Then you can use `docker compose up` to start the application (s).
3. Connect to your Grafana instance. Per default, it is available at http://localhost:3000.
4. In Grafana, create a new datasource with the following settings:
   1. Type: InfluxDB
   2. URL: http://d2stat-influxdb:8086
   3. Database: telegraf
   4. User: telegraf
   5. Password: password (change this in the .env file)
   6. Example image can be found here: [grafana-datasource.png](doc%2Fgrafana-datasource.png)
5. Create new Dashboards and have fun!
   1. Here is an example dashboard (in json format) which you can import: [example-dashboard.json](doc%2Fexample-dashboard.json)



## Contribution
Feel free to contribute to this project. Just open a pull request. 
For a quick start, head over to the [issues](https://github.com/Mijago/D2StatCollector/issues?q=is%3Aissue+is%3Aopen+label%3A"good+first+issue") with the label "good first issue". Thanks!

Build the image with `docker build --target final -f D2StatCollector.Server/Dockerfile -t d2stat-collector:dev .` and change the image in the docker-compose file to `d2stat-collector:dev`.

