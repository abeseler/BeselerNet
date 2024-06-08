/* Migration
{
    "title": "app_reader:permissions",
    "runOnChange": true
}
*/
GRANT USAGE ON SCHEMA app TO app_reader;
GRANT SELECT ON ALL TABLES IN SCHEMA app TO app_reader;

ALTER DEFAULT PRIVILEGES IN SCHEMA app
GRANT SELECT ON TABLES TO app_reader;
