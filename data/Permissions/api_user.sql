/* Migration
{
    "title": "api_user:permissions",
    "runOnChange": true
}
*/
GRANT CONNECT ON DATABASE bslr TO api_user;
GRANT app_reader TO api_user;
GRANT app_writer TO api_user;
