CREATE SCHEMA elsa_register_db;
USE elsa_register_db;

DROP TABLE user;

CREATE TABLE user
(
    email VARCHAR(30) PRIMARY KEY,
    name VARCHAR(25) NOT NULL,
    created DATETIME
);

SELECT * FROM user;
DELETE FROM user;