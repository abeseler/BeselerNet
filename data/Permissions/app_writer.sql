/* Migration
{
    "title": "app_writer:permissions",
    "runOnChange": true
}
*/
GRANT USAGE ON SCHEMA app TO app_writer;
GRANT INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA app TO app_writer;

ALTER DEFAULT PRIVILEGES IN SCHEMA app
GRANT INSERT, UPDATE, DELETE ON TABLES TO app_writer;
