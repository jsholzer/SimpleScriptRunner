create table tractor
(
    `make` varchar(255) not null,
    `model` varchar(255) not null,
    `color` varchar(255) not null,
    primary key (`make`,`model`,`color`)
);