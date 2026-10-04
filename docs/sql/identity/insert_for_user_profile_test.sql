use
`ms_identity_test`;
START TRANSACTION;

INSERT INTO `user_profile` (`user_profile_account_id`, `user_profile_first_name`, `user_profile_last_name`,
                            `user_profile_gender`, `user_profile_date_of_birth`, `user_profile_phone_number`)
VALUES ('01a051c7-348f-7dc8-9f4a-d9efcd9171e3', 'John', 'Doe', 'male', '1990-01-01', '0984242001'),
       ('01a051c7-348f-7f88-9c23-9c1c8b0ea021', 'Jane', 'Manager', 'female', '2000-01-12', '0843220324'),
       ('01a051c7-3490-701c-ac2a-28bc50582f30', 'Lilith', '', 'female', '2003-11-12', '0843220326'),
       ('01a051c7-3490-702c-944f-b6b57ab43f9f', 'Bob', 'Smith', 'male', '1995-05-20', '0843210324'),
       ('01a051c7-3490-7034-b170-58e933015301', 'Alice', 'Smith', 'female', '2005-05-20', '0843220024'),
       ('01a051c7-3490-7040-84fe-d385f1e01893', 'Tom', 'Halland', 'male', '2005-01-20', '0841210324'),
       ('01a051c7-3490-7048-a9eb-cea8b71457d4', 'Thien', 'Lang', 'male', '2005-01-20', '0843120314'),
       ('01a052fc-efb9-7ef1-b57f-029e779396de', 'Dien', 'Vy', 'unspecified', '2002-01-20', null);
COMMIT;


alter table user_profile
    modify column user_profile_address varchar (255) not null default '';

show
columns from user_profile;
select *
from user_profile;
select *
from account;

