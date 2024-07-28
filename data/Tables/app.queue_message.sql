/* Migration
{
    "title": "app.queue:createTable"
}
*/
CREATE TABLE IF NOT EXISTS app.queue_message (
    id uuid NOT NULL,
    queue_id INT NOT NULL,
    payload JSON NOT NULL,
    created_on TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    expires_on TIMESTAMP NOT NULL,
    received_on TIMESTAMP NULL,
    invisible_until TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    receive_count INT NOT NULL,
    CONSTRAINT pk_queue_message PRIMARY KEY (id)
);

/* Migration
{
    "title": "app.queue_message:createIndex1"
}
*/
CREATE INDEX IF NOT EXISTS idx_queue_message_lookup ON app.queue_message (queue_id, invisible_until);
