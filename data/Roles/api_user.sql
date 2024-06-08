/* Migration
{
    "title": "api_user:createRole"
}
*/
do
$$
BEGIN
  IF NOT EXISTS (SELECT * FROM pg_user WHERE usename = 'api_user') THEN
     CREATE ROLE api_user WITH LOGIN PASSWORD 'api_user' INHERIT;
  END IF;
END
$$;
