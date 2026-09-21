-- Fresh schema for MariaDB 11.7+ (UUID_v7). Run against an empty database.

CREATE DATABASE IF NOT EXISTS `ms_identity_test`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `ms_identity_test`;

# Identity module
CREATE TABLE `account`
(
    `account_id`         UUID PRIMARY KEY      DEFAULT (UUID_v7()),
    `account_email`      VARCHAR(255) NOT NULL,
    `account_password`   VARCHAR(255) NOT NULL,
    `account_created_at` TIMESTAMP             DEFAULT CURRENT_TIMESTAMP,
    `account_updated_at` TIMESTAMP             DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `account_is_active`  BOOLEAN      NOT NULL DEFAULT TRUE,

    UNIQUE INDEX `idx_account_email` (`account_email`),
    INDEX                `idx_account_is_active_account_id` (`account_is_active`, `account_id`)
) ENGINE = InnoDB;

CREATE TABLE `role`
(
    `role_id`          UUID PRIMARY KEY      DEFAULT (UUID_v7()),
    `role_code`        VARCHAR(50)  NOT NULL,
    `role_name`        VARCHAR(150) NOT NULL,
    `role_description` VARCHAR(300),
    `role_is_active`   BOOLEAN      NOT NULL DEFAULT TRUE,
    `role_created_at`  TIMESTAMP             DEFAULT CURRENT_TIMESTAMP,
    `role_updated_at`  TIMESTAMP             DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    UNIQUE INDEX `idx_role_code` (`role_code`),
    UNIQUE INDEX `idx_role_name` (`role_name`)
) ENGINE = InnoDB;

CREATE TABLE `permission`
(
    `permission_id`          UUID PRIMARY KEY      DEFAULT (UUID_v7()),
    `permission_code`        VARCHAR(50)  NOT NULL,
    `permission_name`        VARCHAR(150) NOT NULL,
    `permission_description` VARCHAR(300),
    `permission_is_active`   BOOLEAN      NOT NULL DEFAULT TRUE,
    `permission_created_at`  TIMESTAMP             DEFAULT CURRENT_TIMESTAMP,
    `permission_updated_at`  TIMESTAMP             DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    UNIQUE INDEX `idx_permission_code` (`permission_code`),
    UNIQUE INDEX `idx_permission_name` (`permission_name`)
) ENGINE = InnoDB;

CREATE TABLE `account_role`
(
    `account_id`  UUID NOT NULL,
    `role_id`     UUID NOT NULL,
    `assigned_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`account_id`, `role_id`),
    INDEX         `idx_account_role_role_id` (`role_id`),

    CONSTRAINT `fk_account_role_account`
        FOREIGN KEY (`account_id`) REFERENCES `account` (`account_id`),
    CONSTRAINT `fk_account_role_role`
        FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`)
) ENGINE = InnoDB;

CREATE TABLE `role_permission`
(
    `role_id`       UUID NOT NULL,
    `permission_id` UUID NOT NULL,
    `assigned_at`   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`role_id`, `permission_id`),
    INDEX           `idx_role_permission_permission_id` (`permission_id`),

    CONSTRAINT `fk_role_permission_role`
        FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`),
    CONSTRAINT `fk_role_permission_permission`
        FOREIGN KEY (`permission_id`) REFERENCES `permission` (`permission_id`)
) ENGINE = InnoDB;

CREATE TABLE `account_additional_permission`
(
    `account_id`    UUID NOT NULL,
    `permission_id` UUID NOT NULL,
    `assigned_at`   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`account_id`, `permission_id`),
    INDEX           `idx_account_additional_permission_permission_id` (`permission_id`),

    CONSTRAINT `fk_account_additional_permission_account`
        FOREIGN KEY (`account_id`) REFERENCES `account` (`account_id`),
    CONSTRAINT `fk_account_additional_permission_permission`
        FOREIGN KEY (`permission_id`) REFERENCES `permission` (`permission_id`)
) ENGINE = InnoDB;

CREATE TABLE `user_profile`
(
    `user_profile_id`            VARCHAR(12) NOT NULL PRIMARY KEY,
    `user_profile_account_id`    UUID NOT NULL,
    `user_profile_first_name`    VARCHAR(30),
    `user_profile_last_name`     VARCHAR(30),
    `user_profile_date_of_birth` DATE,
    `user_profile_gender`        ENUM ('male', 'female', 'unspecified') NOT NULL DEFAULT 'unspecified',
    `user_profile_phone_number`  VARCHAR(10),
    `user_profile_address`       VARCHAR(255) NOT NULL DEFAULT '',
    `user_profile_avatar_url`    VARCHAR(255),
    `user_profile_created_at`    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `user_profile_updated_at`    TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    UNIQUE INDEX `idx_user_profile_account_id` (`user_profile_account_id`),
    INDEX                         `idx_user_profile_phone_number` (`user_profile_phone_number`),
    INDEX                         `idx_user_profile_date_of_birth` (`user_profile_date_of_birth`),

    CONSTRAINT `fk_user_profile_account`
        FOREIGN KEY (`user_profile_account_id`) REFERENCES `account` (`account_id`)
) ENGINE = InnoDB;


# Premise module
CREATE TABLE `location`
(
    `location_id`        UUID PRIMARY KEY DEFAULT (UUID_v7()),
    `location_address`   VARCHAR(255) NOT NULL,
    `location_created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `location_updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    INDEX `idx_location_address`
        (`location_address`)
) ENGINE = InnoDB;

CREATE TABLE `premise`
(
    `premise_id`          UUID PRIMARY KEY DEFAULT (UUID_v7()),
    `premise_name`        VARCHAR(50) NOT NULL,
    `premise_location_id` UUID NOT NULL,

    `premise_status` ENUM (
        'reserved',
        'rented',
        'available',
        'maintenance'
        ) NOT NULL DEFAULT 'available',

    `premise_position`     INT NOT NULL,
    `premise_floor`        INT NOT NULL,
    `premise_area`         VARCHAR(10) NOT NULL DEFAULT '',
    `premise_description`  VARCHAR(100),
    `premise_created_at`   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `premise_updated_at`   TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    INDEX `idx_premise_name` (`premise_name`(20)),

    CONSTRAINT `fk_premise_location`
        FOREIGN KEY (`premise_location_id`)
            REFERENCES `location` (`location_id`)
) ENGINE = InnoDB;

CREATE TABLE `business_type`
(
    `business_type_id`          UUID PRIMARY KEY DEFAULT (UUID_v7()),
    `business_type_name`        VARCHAR(50) UNIQUE NOT NULL,
    `business_type_description` VARCHAR(255),
    `business_type_is_active`   BOOLEAN NOT NULL DEFAULT TRUE,
    `business_type_created_at`  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `business_type_updated_at`  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    INDEX `idx_business_type_name`
        (`business_type_name`(20))
) ENGINE = InnoDB;

CREATE TABLE `premise_business_type`
(
    `pre_bt_premise_id`       UUID NOT NULL,
    `pre_bt_business_type_id` UUID NOT NULL,
    `pre_bt_assigned_at`          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`pre_bt_premise_id`, `pre_bt_business_type_id`),

    CONSTRAINT `fk_premise_business_type_premise`
        FOREIGN KEY (`pre_bt_premise_id`)
            REFERENCES `premise` (`premise_id`),

    CONSTRAINT `fk_premise_business_type_business_type`
        FOREIGN KEY (`pre_bt_business_type_id`)
            REFERENCES `business_type` (`business_type_id`)
) ENGINE = InnoDB;

CREATE TABLE `whitelist_product`
(
    `whitelist_product_id`          UUID PRIMARY KEY DEFAULT (UUID_v7()),
    `whitelist_product_name`        VARCHAR(100) NOT NULL,
    `whitelist_product_description` VARCHAR(255),
    `whitelist_product_created_at`  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `whitelist_product_updated_at`  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    INDEX `idx_whitelist_product_name`
        (`whitelist_product_name`)
) ENGINE = InnoDB;

CREATE TABLE `product_business_type`
(
    `pbt_product_id`       UUID NOT NULL,
    `pbt_business_type_id` UUID NOT NULL,
    `pbt_assigned_at`          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`pbt_product_id`, `pbt_business_type_id`),

    CONSTRAINT `fk_product_business_type_product`
        FOREIGN KEY (`pbt_product_id`)
            REFERENCES `whitelist_product` (`whitelist_product_id`),

    CONSTRAINT `fk_product_business_type_business_type`
        FOREIGN KEY (`pbt_business_type_id`)
            REFERENCES `business_type` (`business_type_id`)
) ENGINE = InnoDB;

CREATE TABLE `premise_media`
(
    `premise_media_id`         INT PRIMARY KEY AUTO_INCREMENT,
    `premise_media_premise_id` UUID NOT NULL,
    `premise_media_image_url`      VARCHAR(255) NOT NULL,
    `premise_media_created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `premise_media_updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT `fk_premise_media_premise`
        FOREIGN KEY (`premise_media_premise_id`)
            REFERENCES `premise` (`premise_id`)
) ENGINE = InnoDB;

# Contract module
CREATE TABLE `contract`
(
    `contract_id`                  UUID PRIMARY KEY DEFAULT (UUID_v7()),
    `contract_account_id`          UUID NOT NULL,
    `contract_deposit`             DECIMAL(18,2) NOT NULL,
    `contract_rental_price`        DECIMAL(18,2) NOT NULL,
    `contract_premise_return_date` TIMESTAMP NOT NULL,

    `contract_status` ENUM (
        'pending_approval',
        'pending_signature',
        'canceled',
        'expired',
        'signed',
        'terminated'
        ) NOT NULL DEFAULT 'pending_approval',

    `contract_termination_date`    TIMESTAMP,
    `contract_created_at`          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `contract_updated_at`          TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT `fk_contract_account`
        FOREIGN KEY (`contract_account_id`)
            REFERENCES `account` (`account_id`)
) ENGINE = InnoDB;

# (write on premise modules)
# not need assigned at because the premise can only be rented by one contract at a time,
# so we can just use the contract's created_at as the assigned_at
CREATE TABLE `rented_premise`
(
    `rented_premise_contract_id` UUID NOT NULL,
    `rented_premise_premise_id`  UUID NOT NULL,

    PRIMARY KEY (`rented_premise_premise_id`, `rented_premise_contract_id`),

    CONSTRAINT `fk_rented_premise_contract`
        FOREIGN KEY (`rented_premise_contract_id`)
            REFERENCES `contract` (`contract_id`),

    CONSTRAINT `fk_rented_premise_premise`
        FOREIGN KEY (`rented_premise_premise_id`)
            REFERENCES `premise` (`premise_id`)
) ENGINE = InnoDB;

#add fine amount, active status
CREATE TABLE `regulation`
(
    `regulation_id`          UUID PRIMARY KEY DEFAULT (UUID_v7()),
    `regulation_name`        VARCHAR(50) NOT NULL,
    `regulation_description` VARCHAR(255),
    `regulation_fine_amount`       DECIMAL(18,2),
    `regulation_is_active`   BOOLEAN NOT NULL DEFAULT TRUE,
    `regulation_created_at`  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `regulation_updated_at`  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    INDEX `idx_regulation_name`
        (`regulation_name`(20))
) ENGINE = InnoDB;

CREATE TABLE `contract_regulation`
(
    `regulation_id` UUID NOT NULL,
    `contract_id`   UUID NOT NULL,
    `assigned_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`regulation_id`, `contract_id`),

    CONSTRAINT `fk_contract_regulation_regulation`
        FOREIGN KEY (`regulation_id`)
            REFERENCES `regulation` (`regulation_id`),

    CONSTRAINT `fk_contract_regulation_contract`
        FOREIGN KEY (`contract_id`)
            REFERENCES `contract` (`contract_id`)
) ENGINE = InnoDB;

# historical violation records for each contract, including violation content, penalty amount, due date, and violation date,
# remove old enum column (`violation_status`) and replace with a boolean column (`violation_is_resolved`) to indicate whether the violation has been resolved or not,
# not let violation date and due date be null, because we need to know when the violation happened and when it is due,
#default violation date to current timestamp and due date 7 days later

CREATE TABLE `contract_violation`
(
    `contract_violation_id`                  UUID PRIMARY KEY DEFAULT (UUID_v7()),
    `contract_id`          UUID NOT NULL,
    `violation_content`              VARCHAR(150) NOT NULL,
    `violation_penalty_amount`  DECIMAL(18,2),
    `violation_date`                 TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `violation_due_date`                  TIMESTAMP NOT NULL DEFAULT (CURRENT_TIMESTAMP + INTERVAL 7 DAY),
    `violation_is_resolved`              BOOLEAN NOT NULL DEFAULT FALSE,

    INDEX `idx_contract_violation_content`
        (`violation_content`(20)),

    INDEX `idx_contract_violation_date`
        (`violation_date`),

    CONSTRAINT `fk_contract_violation_contract`
        FOREIGN KEY (`contract_id`)
            REFERENCES `contract` (`contract_id`)
) ENGINE = InnoDB;

# add a new table to store monthly invoices for each contract, including invoice details such as payment date, due date, total amount, and status (paid/unpaid/overdue)
# let payment date and due date be not null, because we need to know when the invoice is generated and when it is due,
# default payment date to current timestamp and due date to 30 days later

CREATE TABLE `monthly_invoice`
(
    `invoice_id`           UUID PRIMARY KEY DEFAULT (UUID_v7()),
    `invoice_contract_id`  UUID NOT NULL,
    `invoice_payment_date` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `invoice_due_date`     TIMESTAMP NOT NULL DEFAULT (CURRENT_TIMESTAMP + INTERVAL 30 DAY),
    `invoice_total_amount`  DECIMAL(18,2),
    `invoice_status` ENUM (
        'unpaid',
        'paid',
        'overdue'
        ) NOT NULL DEFAULT 'unpaid',
    `invoice_created_at`   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `invoice_updated_at`   TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    INDEX `idx_invoice_payment_date`
        (`invoice_payment_date`),

    INDEX `idx_invoice_due_date`
        (`invoice_due_date`),

    CONSTRAINT `fk_monthly_invoice_contract`
        FOREIGN KEY (`invoice_contract_id`)
            REFERENCES `contract` (`contract_id`)
) ENGINE = InnoDB;

# add a new table to store invoice details for each premise in the invoice, including rental price, electricity fee, water fee, garbage fee, and total amount
# the table should have a foreign key to the monthly_invoice table and the premise table,
# do not let the total amount be null, because we need to know how much the tenant has to pay for each premise in the invoice,
# default total amount to the sum of rental price, electricity fee, water fee, and garbage

CREATE TABLE `invoice_detail`
(
    `invoice_detail_id`              INT PRIMARY KEY AUTO_INCREMENT,
    `invoice_detail_invoice_id`      UUID NOT NULL,
    `invoice_detail_premise_id`      UUID NOT NULL,
    `invoice_detail_rental_price`    DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `invoice_detail_electricity_fee` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `invoice_detail_water_fee`       DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `invoice_detail_garbage_fee`     DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `invoice_detail_total_amount`    DECIMAL(18,2) NOT NULL DEFAULT (
        `invoice_detail_rental_price` +
        `invoice_detail_electricity_fee` +
        `invoice_detail_water_fee` +
        `invoice_detail_garbage_fee`
        ),

    CONSTRAINT `fk_invoice_detail_invoice`
        FOREIGN KEY (`invoice_detail_invoice_id`)
            REFERENCES `monthly_invoice` (`invoice_id`),

    CONSTRAINT `fk_invoice_detail_premise`
        FOREIGN KEY (`invoice_detail_premise_id`)
            REFERENCES `premise` (`premise_id`)
) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `receipt`
(
    `receipt_id`                         UUID PRIMARY KEY DEFAULT (UUID_v7()),
    `receipt_invoice_id`                UUID NOT NULL,
    `receipt_created_by_account_id`      UUID NOT NULL,
    `receipt_payment_method`             ENUM(
        'vnpay',
        'bank',
        'cash'
        ) NOT NULL DEFAULT 'cash',
    `receipt_amount`                     DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `receipt_payment_date`               TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `receipt_transaction_reference`      VARCHAR(100) NULL,
    `receipt_gateway_transaction_number` VARCHAR(100) NULL,
    `receipt_transaction_info`           VARCHAR(250) NULL,
    `receipt_bank_name`                  VARCHAR(100) NULL,
    `receipt_created_at`                 TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `receipt_updated_at`                 TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    UNIQUE KEY `uk_receipt_invoice_id` (`receipt_invoice_id`),

    INDEX `idx_receipt_payment_method` (`receipt_payment_method`),
    INDEX `idx_receipt_payment_date` (`receipt_payment_date`),
    INDEX `idx_receipt_transaction_reference` (`receipt_transaction_reference`),
    INDEX `idx_receipt_created_by_account_id` (`receipt_created_by_account_id`),

    CONSTRAINT `fk_receipt_invoice`
        FOREIGN KEY (`receipt_invoice_id`)
            REFERENCES `monthly_invoice` (`invoice_id`),

    CONSTRAINT `fk_receipt_created_by_account`
        FOREIGN KEY (`receipt_created_by_account_id`)
            REFERENCES `account` (`account_id`)

) ENGINE = InnoDB;

# Ticket and Notification module (now is low priority, so we will implement it later)
# remove old enum column (`ticket_status`) and replace with a boolean column (`ticket_is_resolved`) to indicate whether the ticket has been resolved or not
CREATE TABLE `ticket`
(
    `ticket_id`        UUID PRIMARY KEY DEFAULT (UUID_v7()),
    `ticket_account_id` UUID NOT NULL,
    `ticket_content`   VARCHAR(255) NOT NULL,

    `ticket_type` ENUM (
        'review',
        'complaint',
        'violation',
        'feedback'
        ) NOT NULL DEFAULT 'feedback',

    `ticket_is_resolved` BOOLEAN NOT NULL DEFAULT FALSE,

    `ticket_created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `ticket_updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT `fk_ticket_account`
        FOREIGN KEY (`ticket_account_id`)
            REFERENCES `account` (`account_id`)
) ENGINE = InnoDB;

CREATE TABLE `ticket_media`
(
    `ticket_media_id`        INT PRIMARY KEY AUTO_INCREMENT,
    `ticket_media_ticket_id` UUID NOT NULL,
    `ticket_media_image_url`     VARCHAR(255) NOT NULL,

    CONSTRAINT `fk_ticket_media_ticket`
        FOREIGN KEY (`ticket_media_ticket_id`)
            REFERENCES `ticket` (`ticket_id`)
) ENGINE = InnoDB;

# Notification,
# move `notification_status` column to `notification_is_read` boolean column to indicate whether the notification has been read or not
CREATE TABLE `notification`
(
    `notification_id` UUID PRIMARY KEY DEFAULT (UUID_v7()),

    `notification_sender_account_id` UUID NOT NULL,

    `notification_type` ENUM (
        'violation',
        'ticket',
        'other'
        ) NOT NULL DEFAULT 'other',

    `notification_content` VARCHAR(255) NOT NULL,
    `notification_is_read` BOOLEAN NOT NULL DEFAULT FALSE,

    `notification_created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `notification_updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT `fk_notification_sender_account`
        FOREIGN KEY (`notification_sender_account_id`)
            REFERENCES `account` (`account_id`)
) ENGINE = InnoDB;

CREATE TABLE `notification_recipient`
(
    `nr_notification_id` UUID NOT NULL,
    `nr_account_id`      UUID NOT NULL, 

    PRIMARY KEY (`nr_notification_id`, `nr_account_id`),

    CONSTRAINT `fk_notification_recipient_notification`
        FOREIGN KEY (`nr_notification_id`)
            REFERENCES `notification` (`notification_id`),

    CONSTRAINT `fk_notification_recipient_account`
        FOREIGN KEY (`nr_account_id`)
            REFERENCES `account` (`account_id`)
) ENGINE = InnoDB;

# Triggers to enforce immutability of certain fields
DELIMITER //

CREATE TRIGGER `trigger_permission_code_immutable`
    BEFORE UPDATE
    ON `permission`
    FOR EACH ROW
BEGIN
    IF NOT (OLD.`permission_code` <=> NEW.`permission_code`) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Permission code cannot be changed';
    END IF;
END //

CREATE TRIGGER `trigger_role_code_immutable`
    BEFORE UPDATE
    ON `role`
    FOR EACH ROW
BEGIN
    IF NOT (OLD.`role_code` <=> NEW.`role_code`) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Role code cannot be changed';
    END IF;
END //

DELIMITER ;

