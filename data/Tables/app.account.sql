/* Migration
{
    "title": "app.account:createTable"
}
*/
CREATE TABLE IF NOT EXISTS app.account (
    account_id INT GENERATED ALWAYS AS IDENTITY,
    email VARCHAR(50) NOT NULL,
    given_name VARCHAR(50) NULL,
    family_name VARCHAR(50) NULL,
    secret_hash TEXT NULL,
    is_locked BOOLEAN NOT NULL DEFAULT (FALSE),
    is_verified BOOLEAN NOT NULL DEFAULT (FALSE),
    created_on TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    last_login_on TIMESTAMP NULL,
    failed_login_attempts INT NOT NULL DEFAULT (0),
    CONSTRAINT pk_account PRIMARY KEY (account_id),
    CONSTRAINT uq_account_email UNIQUE (email)
);
