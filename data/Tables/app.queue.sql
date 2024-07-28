/* Migration
{
    "title": "app.queue:createTable"
}
*/
CREATE TABLE IF NOT EXISTS app.queue (
    queue_id uuid NOT NULL,
    queue_name VARCHAR(100) NOT NULL,
    message_retention_days INT NOT NULL,
    message_max_receives INT NOT NULL,
    created_on TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    dead_letter_queue_id INT NULL,
    CONSTRAINT pk_queue PRIMARY KEY (queue_id),
    CONSTRAINT uq_queue UNIQUE (queue_name)
);
