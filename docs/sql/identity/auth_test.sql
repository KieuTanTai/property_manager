-- Fresh schema for MariaDB 11.7+ (UUID_v7). Run against an empty database.

CREATE
DATABASE IF NOT EXISTS `ms_identity_test`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE
`ms_identity_test`;

CREATE TABLE `account`
(
    `account_id`         UUID PRIMARY KEY      DEFAULT (UUID_v7()),
    `account_email`      VARCHAR(255) NOT NULL,
    `account_password`   VARCHAR(255) NOT NULL,
    `account_created_at` TIMESTAMP             DEFAULT (NOW()),
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
    `role_created_at`  TIMESTAMP             DEFAULT (NOW()),
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
    `permission_created_at`  TIMESTAMP             DEFAULT (NOW()),
    `permission_updated_at`  TIMESTAMP             DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    UNIQUE INDEX `idx_permission_code` (`permission_code`),
    UNIQUE INDEX `idx_permission_name` (`permission_name`)
) ENGINE = InnoDB;

CREATE TABLE `account_role`
(
    `account_id`  UUID NOT NULL,
    `role_id`     UUID NOT NULL,
    `assigned_at` TIMESTAMP DEFAULT (NOW()),

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
    `assigned_at`   TIMESTAMP DEFAULT (NOW()),

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
    `assigned_at`   TIMESTAMP DEFAULT (NOW()),

    PRIMARY KEY (`account_id`, `permission_id`),
    INDEX           `idx_account_additional_permission_permission_id` (`permission_id`),

    CONSTRAINT `fk_account_additional_permission_account`
        FOREIGN KEY (`account_id`) REFERENCES `account` (`account_id`),
    CONSTRAINT `fk_account_additional_permission_permission`
        FOREIGN KEY (`permission_id`) REFERENCES `permission` (`permission_id`)
) ENGINE = InnoDB;

CREATE TABLE `user_profile`
(
    `user_profile_id`            VARCHAR(12)  NOT NULL PRIMARY KEY,
    `user_profile_account_id`    UUID         NOT NULL,
    `user_profile_first_name`    VARCHAR(30),
    `user_profile_last_name`     VARCHAR(30),
    `user_profile_date_of_birth` DATE,
    `user_profile_gender`        ENUM ('male', 'female', 'unspecified') NOT NULL DEFAULT 'unspecified',
    `user_profile_phone_number`  VARCHAR(10),
    `user_profile_address`       VARCHAR(255) NOT NULL DEFAULT '',
    `user_profile_avatar_url`    VARCHAR(255),
    `user_profile_created_at`    TIMESTAMP             DEFAULT (NOW()),
    `user_profile_updated_at`    TIMESTAMP             DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    UNIQUE INDEX `idx_user_profile_account_id` (`user_profile_account_id`),
    INDEX                        `idx_user_profile_phone_number` (`user_profile_phone_number`),
    INDEX                        `idx_user_profile_date_of_birth` (`user_profile_date_of_birth`),

    CONSTRAINT `fk_user_profile_account`
        FOREIGN KEY (`user_profile_account_id`) REFERENCES `account` (`account_id`)
) ENGINE = InnoDB;

DELIMITER
//

CREATE TRIGGER `trigger_permission_code_immutable`
    BEFORE UPDATE
    ON `permission`
    FOR EACH ROW
BEGIN
    IF NOT (OLD.`permission_code` <=> NEW.`permission_code`) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Permission code cannot be changed';
END IF;
END
//

CREATE TRIGGER `trigger_role_code_immutable`
    BEFORE UPDATE
    ON `role`
    FOR EACH ROW
BEGIN
    IF NOT (OLD.`role_code` <=> NEW.`role_code`) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Role code cannot be changed';
END IF;
END
//

DELIMITER ;