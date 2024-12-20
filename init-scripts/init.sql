-- init.sql
CREATE USER IF NOT EXISTS 'hashkorea'@'%' IDENTIFIED BY 'hashkorea';
GRANT ALL PRIVILEGES ON HashKorea.* TO 'hashkorea'@'%';
FLUSH PRIVILEGES;