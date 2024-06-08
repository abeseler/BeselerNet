/* Migration
{
    "title": "app.token_log:createTable"
}
*/
CREATE TABLE IF NOT EXISTS app.token_log (
    token_log_id INT GENERATED ALWAYS AS IDENTITY,
    token_id uuid NOT NULL,
    replaced_by_token_id uuid NULL,
    created_on TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    expires_on TIMESTAMP NOT NULL,
    is_revoked BOOLEAN NOT NULL DEFAULT (FALSE),
    account_id INT NOT NULL,
    CONSTRAINT pk_token_log PRIMARY KEY (token_log_id)
);

/* Migration
{
    "title": "app.token_log:createIndex1"
}
*/
CREATE INDEX ix_token_log_account_id ON app.token_log (account_id);

/* Migration
{
    "title": "app.token_log:createIndex2"
}
*/
CREATE INDEX ix_token_log_replaced_by_token_id ON app.token_log (replaced_by_token_id)
WHERE replaced_by_token_id IS NOT NULL;
