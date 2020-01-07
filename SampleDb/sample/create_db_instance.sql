--
-- Manually run these steps
--
create database simple_script_runner_sample;

use mysql;

CREATE USER 'simplescriptadmin'@'%' IDENTIFIED BY 'abc123';
GRANT ALL ON `simple_script_runner_%`.* TO 'simplescriptadmin'@'%';
flush privileges;
