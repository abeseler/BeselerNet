/* Migration
{
    "title": "app.outbox_queue:createTable"
}
*/
CREATE TABLE IF NOT EXISTS app.outbox_queue (
    id INT GENERATED ALWAYS AS IDENTITY,
    service_id VARCHAR(50) NOT NULL,
    payload JSON NOT NULL,
    created_on TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    invisible_until TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    receives_remaining INT NOT NULL DEFAULT (10)
);

/* Migration
{
    "title": "app.outbox_queue:createIndex1"
}
*/
CREATE INDEX IF NOT EXISTS ix_outbox_queue_lookup ON app.outbox_queue (service_id, invisible_until)
WHERE receives_remaining > 0;
