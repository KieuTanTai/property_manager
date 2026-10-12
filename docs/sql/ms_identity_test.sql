/*M!999999\- enable the sandbox mode */ 
-- MariaDB dump 10.20-13.0.2-MariaDB, for Linux (x86_64)
--
-- Host: localhost    Database: ms_identity_test
-- ------------------------------------------------------
-- Server version	13.0.2-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*M!100616 SET @OLD_NOTE_VERBOSITY=@@NOTE_VERBOSITY, NOTE_VERBOSITY=0 */;

--
-- Table structure for table `account`
--

DROP TABLE IF EXISTS `account`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `account` (
  `account_id` uuid NOT NULL DEFAULT uuid_v7(),
  `account_email` varchar(255) NOT NULL,
  `account_password` varchar(255) NOT NULL,
  `account_created_at` timestamp NULL DEFAULT current_timestamp(),
  `account_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `account_is_active` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`account_id`),
  UNIQUE KEY `idx_account_email` (`account_email`),
  KEY `idx_account_is_active_account_id` (`account_is_active`,`account_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `account`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `account` WRITE;
/*!40000 ALTER TABLE `account` DISABLE KEYS */;
INSERT INTO `account` VALUES
('01a051c7-348f-7dc8-9f4a-d9efcd9171e3','admin@test.local','AQAAAAIAAYagAAAAELixh0FWBwa6Sawyvj9fKKiTK6whriWSSPNZL8wKdmNOV2SyTupChYSDiBlGYiMpDQ==','2026-08-30 08:26:44','2026-09-18 06:05:16',1),
('01a051c7-348f-7f88-9c23-9c1c8b0ea021','manager@test.local','AQAAAAIAAYagAAAAEPSt2P0Fq9rO2CZZk3YhBl0a2xrIX7MOSQkqEC6hhX5CWA58fTSOm0nwteM83mrxxA==','2026-08-30 08:26:44','2026-09-18 06:05:16',1),
('01a051c7-3490-701c-ac2a-28bc50582f30','inspector@test.local','AQAAAAIAAYagAAAAEGcpuayGA2RevPIgd5y1es9joUEBajnNkjmrbX56e8a2JR4Xon0aew1x3qpYAWSOFA==','2026-08-30 08:26:44','2026-09-21 14:47:24',1),
('01a051c7-3490-702c-944f-b6b57ab43f9f','employee01@test.local','AQAAAAIAAYagAAAAEMU8FmdZkQGnNmfMURqSX+7i3a0Brf/x4fLfg3EaKflGffgL2X9hntkCbq38E7l68g==','2026-08-30 08:26:44','2026-09-18 06:05:16',1),
('01a051c7-3490-7034-b170-58e933015301','employee02@test.local','AQAAAAIAAYagAAAAECPfb5OPbKB9eK9qMgwile9qZcqw96EAloQxHFPfT/JAQbpEBoDefUTBYj7CYTipcA==','2026-08-30 08:26:44','2026-09-18 06:05:16',1),
('01a051c7-3490-7040-84fe-d385f1e01893','customer01@test.local','AQAAAAIAAYagAAAAEJfVCF8CQPNuBTrR8hOGj8sZEOzurAw3S0Oqt2DIH0wW+WknwtlTZscswNoEbJqKBw==','2026-08-30 08:26:44','2026-09-18 06:05:16',1),
('01a051c7-3490-7048-a9eb-cea8b71457d4','customer02@test.local','AQAAAAIAAYagAAAAEP3zy8BywKaKrfxue3hwA1wpNlc8+c0JY2A4R0am+57O0KzMjYsNieqauo3ZLDnTcQ==','2026-08-30 08:26:44','2026-09-18 06:05:16',1),
('01a052fc-efb9-7ef1-b57f-029e779396de','customer_demo2@example.com','AQAAAAIAAYagAAAAEIPCSSuJxrPB43vZ3e+4ZNfs0u8RCQxrlIK9262lUeQKcbu6CnNJEqCSVYAWRs04/Q==','2026-08-30 14:05:03','2026-09-18 06:05:16',0),
('01a0625e-cd9b-7a75-8564-c2e007198961','demoaccount@example.com','AQAAAAIAAYagAAAAEL0l1ctyd8LsMxGPUosC3pfawZ7x+wjRqMwEuHbi0qpw9dvbf3oI2IivaY1rb/cyuw==','2026-09-02 13:46:15','2026-09-18 06:05:16',1),
('01a06264-ec47-78c7-a994-186d29a58f9c','demo_account01@example.com','AQAAAAIAAYagAAAAEAoRwgB7d/eozvq0agkKNv6FuG/QtVp+a1qY9Qwfs/sXDwXqZQAluZddMUigDpXiTA==','2026-09-02 13:52:56','2026-09-18 06:05:16',1),
('01a0626c-0922-7ba0-b2e4-9da849fa7c51','demoaccount2@gmail.com','AQAAAAIAAYagAAAAEAcxxrWrfcayU8JNy9G9ueK19EnAyj4gXw2SvO5I62Drs7LZpcAa5Oz9EyCYzvPbmw==','2026-09-02 14:00:42','2026-09-18 06:05:16',1),
('01a0b2ee-2899-7eb8-bb16-4dc3c6387a2d','tanlang@gmail.com','AQAAAAIAAYagAAAAECYUQKruTMOVVubgi2HdYixGNpZW0A9tGdjySWOjEwkvYPxFitDC93HXAxxGwA9R0Q==','2026-09-18 05:12:27','2026-09-18 06:09:09',1);
/*!40000 ALTER TABLE `account` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `account_additional_permission`
--

DROP TABLE IF EXISTS `account_additional_permission`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `account_additional_permission` (
  `account_id` uuid NOT NULL,
  `permission_id` uuid NOT NULL,
  `assigned_at` timestamp NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`account_id`,`permission_id`),
  KEY `idx_account_additional_permission_permission_id` (`permission_id`),
  CONSTRAINT `fk_account_additional_permission_account` FOREIGN KEY (`account_id`) REFERENCES `account` (`account_id`),
  CONSTRAINT `fk_account_additional_permission_permission` FOREIGN KEY (`permission_id`) REFERENCES `permission` (`permission_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `account_additional_permission`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `account_additional_permission` WRITE;
/*!40000 ALTER TABLE `account_additional_permission` DISABLE KEYS */;
/*!40000 ALTER TABLE `account_additional_permission` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `account_role`
--

DROP TABLE IF EXISTS `account_role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `account_role` (
  `account_id` uuid NOT NULL,
  `role_id` uuid NOT NULL,
  `assigned_at` timestamp NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`account_id`,`role_id`),
  KEY `idx_account_role_role_id` (`role_id`),
  CONSTRAINT `fk_account_role_account` FOREIGN KEY (`account_id`) REFERENCES `account` (`account_id`),
  CONSTRAINT `fk_account_role_role` FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `account_role`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `account_role` WRITE;
/*!40000 ALTER TABLE `account_role` DISABLE KEYS */;
INSERT INTO `account_role` VALUES
('01a051c7-348f-7dc8-9f4a-d9efcd9171e3','01a0c42c-af6b-7a6c-9560-c0f3e79af41e','2026-09-21 14:58:31'),
('01a051c7-348f-7f88-9c23-9c1c8b0ea021','01a0c42c-af6b-7a48-a097-8d0537814615','2026-09-21 14:58:31'),
('01a051c7-3490-701c-ac2a-28bc50582f30','01a0c42c-af6b-7a1c-b6b1-61af0becd83b','2026-09-21 14:58:31'),
('01a051c7-3490-702c-944f-b6b57ab43f9f','01a0c42c-af6b-79bc-95f3-47764309c0e6','2026-09-21 14:58:31'),
('01a051c7-3490-7034-b170-58e933015301','01a0c42c-af6b-79bc-95f3-47764309c0e6','2026-09-21 14:58:31'),
('01a051c7-3490-7040-84fe-d385f1e01893','01a0c42c-af6b-7740-bfe7-619e643d7b6b','2026-09-21 14:58:31'),
('01a051c7-3490-7048-a9eb-cea8b71457d4','01a0c42c-af6b-7740-bfe7-619e643d7b6b','2026-09-21 14:58:31'),
('01a052fc-efb9-7ef1-b57f-029e779396de','01a0c42c-af6b-7740-bfe7-619e643d7b6b','2026-09-21 14:58:31'),
('01a0625e-cd9b-7a75-8564-c2e007198961','01a0c42c-af6b-7740-bfe7-619e643d7b6b','2026-09-21 14:58:31'),
('01a06264-ec47-78c7-a994-186d29a58f9c','01a0c42c-af6b-7740-bfe7-619e643d7b6b','2026-09-21 14:58:31'),
('01a0626c-0922-7ba0-b2e4-9da849fa7c51','01a0c42c-af6b-7740-bfe7-619e643d7b6b','2026-09-21 14:58:31'),
('01a0b2ee-2899-7eb8-bb16-4dc3c6387a2d','01a0c42c-af6b-7740-bfe7-619e643d7b6b','2026-09-21 14:58:31');
/*!40000 ALTER TABLE `account_role` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `business_type`
--

DROP TABLE IF EXISTS `business_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `business_type` (
  `business_type_id` uuid NOT NULL DEFAULT uuid_v7(),
  `business_type_name` varchar(50) NOT NULL,
  `business_type_description` varchar(255) DEFAULT NULL,
  `business_type_is_active` tinyint(1) NOT NULL DEFAULT 1,
  `business_type_created_at` timestamp NULL DEFAULT current_timestamp(),
  `business_type_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`business_type_id`),
  UNIQUE KEY `business_type_name` (`business_type_name`),
  KEY `idx_business_type_name` (`business_type_name`(20))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `business_type`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `business_type` WRITE;
/*!40000 ALTER TABLE `business_type` DISABLE KEYS */;
/*!40000 ALTER TABLE `business_type` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `contract`
--

DROP TABLE IF EXISTS `contract`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `contract` (
  `contract_id` uuid NOT NULL DEFAULT uuid_v7(),
  `contract_account_id` uuid NOT NULL,
  `contract_deposit` decimal(18,2) NOT NULL,
  `contract_rental_price` decimal(18,2) NOT NULL,
  `contract_premise_return_date` timestamp NOT NULL,
  `contract_status` enum('pending_approval','pending_signature','canceled','expired','signed','terminated') NOT NULL DEFAULT 'pending_approval',
  `contract_termination_date` timestamp NULL DEFAULT NULL,
  `contract_created_at` timestamp NULL DEFAULT current_timestamp(),
  `contract_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`contract_id`),
  KEY `fk_contract_account` (`contract_account_id`),
  CONSTRAINT `fk_contract_account` FOREIGN KEY (`contract_account_id`) REFERENCES `account` (`account_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `contract`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `contract` WRITE;
/*!40000 ALTER TABLE `contract` DISABLE KEYS */;
/*!40000 ALTER TABLE `contract` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `contract_regulation`
--

DROP TABLE IF EXISTS `contract_regulation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `contract_regulation` (
  `regulation_id` uuid NOT NULL,
  `contract_id` uuid NOT NULL,
  `assigned_at` timestamp NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`regulation_id`,`contract_id`),
  KEY `fk_contract_regulation_contract` (`contract_id`),
  CONSTRAINT `fk_contract_regulation_contract` FOREIGN KEY (`contract_id`) REFERENCES `contract` (`contract_id`),
  CONSTRAINT `fk_contract_regulation_regulation` FOREIGN KEY (`regulation_id`) REFERENCES `regulation` (`regulation_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `contract_regulation`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `contract_regulation` WRITE;
/*!40000 ALTER TABLE `contract_regulation` DISABLE KEYS */;
/*!40000 ALTER TABLE `contract_regulation` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `contract_violation`
--

DROP TABLE IF EXISTS `contract_violation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `contract_violation` (
  `contract_violation_id` uuid NOT NULL DEFAULT uuid_v7(),
  `contract_id` uuid NOT NULL,
  `violation_content` varchar(150) NOT NULL,
  `violation_penalty_amount` decimal(18,2) DEFAULT NULL,
  `violation_date` timestamp NOT NULL DEFAULT current_timestamp(),
  `violation_due_date` timestamp NOT NULL DEFAULT (current_timestamp() + interval 7 day),
  `violation_is_resolved` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`contract_violation_id`),
  KEY `idx_contract_violation_content` (`violation_content`(20)),
  KEY `idx_contract_violation_date` (`violation_date`),
  KEY `fk_contract_violation_contract` (`contract_id`),
  CONSTRAINT `fk_contract_violation_contract` FOREIGN KEY (`contract_id`) REFERENCES `contract` (`contract_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `contract_violation`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `contract_violation` WRITE;
/*!40000 ALTER TABLE `contract_violation` DISABLE KEYS */;
/*!40000 ALTER TABLE `contract_violation` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `invoice_detail`
--

DROP TABLE IF EXISTS `invoice_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `invoice_detail` (
  `invoice_detail_id` int(11) NOT NULL AUTO_INCREMENT,
  `invoice_detail_invoice_id` uuid NOT NULL,
  `invoice_detail_premise_id` uuid NOT NULL,
  `invoice_detail_rental_price` decimal(18,2) NOT NULL DEFAULT 0.00,
  `invoice_detail_electricity_fee` decimal(18,2) NOT NULL DEFAULT 0.00,
  `invoice_detail_water_fee` decimal(18,2) NOT NULL DEFAULT 0.00,
  `invoice_detail_garbage_fee` decimal(18,2) NOT NULL DEFAULT 0.00,
  `invoice_detail_total_amount` decimal(18,2) NOT NULL DEFAULT (`invoice_detail_rental_price` + `invoice_detail_electricity_fee` + `invoice_detail_water_fee` + `invoice_detail_garbage_fee`),
  PRIMARY KEY (`invoice_detail_id`),
  KEY `fk_invoice_detail_invoice` (`invoice_detail_invoice_id`),
  KEY `fk_invoice_detail_premise` (`invoice_detail_premise_id`),
  CONSTRAINT `fk_invoice_detail_invoice` FOREIGN KEY (`invoice_detail_invoice_id`) REFERENCES `monthly_invoice` (`invoice_id`),
  CONSTRAINT `fk_invoice_detail_premise` FOREIGN KEY (`invoice_detail_premise_id`) REFERENCES `premise` (`premise_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `invoice_detail`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `invoice_detail` WRITE;
/*!40000 ALTER TABLE `invoice_detail` DISABLE KEYS */;
/*!40000 ALTER TABLE `invoice_detail` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `location`
--

DROP TABLE IF EXISTS `location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `location` (
  `location_id` uuid NOT NULL DEFAULT uuid_v7(),
  `location_address` varchar(255) NOT NULL,
  `location_created_at` timestamp NULL DEFAULT current_timestamp(),
  `location_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`location_id`),
  KEY `idx_location_address` (`location_address`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `location`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `location` WRITE;
/*!40000 ALTER TABLE `location` DISABLE KEYS */;
/*!40000 ALTER TABLE `location` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `monthly_invoice`
--

DROP TABLE IF EXISTS `monthly_invoice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `monthly_invoice` (
  `invoice_id` uuid NOT NULL DEFAULT uuid_v7(),
  `invoice_contract_id` uuid NOT NULL,
  `invoice_payment_date` timestamp NOT NULL DEFAULT current_timestamp(),
  `invoice_due_date` timestamp NOT NULL DEFAULT (current_timestamp() + interval 30 day),
  `invoice_total_amount` decimal(18,2) DEFAULT NULL,
  `invoice_status` enum('unpaid','paid','overdue') NOT NULL DEFAULT 'unpaid',
  `invoice_created_at` timestamp NULL DEFAULT current_timestamp(),
  `invoice_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`invoice_id`),
  KEY `idx_invoice_payment_date` (`invoice_payment_date`),
  KEY `idx_invoice_due_date` (`invoice_due_date`),
  KEY `fk_monthly_invoice_contract` (`invoice_contract_id`),
  CONSTRAINT `fk_monthly_invoice_contract` FOREIGN KEY (`invoice_contract_id`) REFERENCES `contract` (`contract_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `monthly_invoice`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `monthly_invoice` WRITE;
/*!40000 ALTER TABLE `monthly_invoice` DISABLE KEYS */;
/*!40000 ALTER TABLE `monthly_invoice` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `notification`
--

DROP TABLE IF EXISTS `notification`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `notification` (
  `notification_id` uuid NOT NULL DEFAULT uuid_v7(),
  `notification_sender_account_id` uuid NOT NULL,
  `notification_type` enum('violation','ticket','other') NOT NULL DEFAULT 'other',
  `notification_content` varchar(255) NOT NULL,
  `notification_is_read` tinyint(1) NOT NULL DEFAULT 0,
  `notification_created_at` timestamp NULL DEFAULT current_timestamp(),
  `notification_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`notification_id`),
  KEY `fk_notification_sender_account` (`notification_sender_account_id`),
  CONSTRAINT `fk_notification_sender_account` FOREIGN KEY (`notification_sender_account_id`) REFERENCES `account` (`account_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notification`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `notification` WRITE;
/*!40000 ALTER TABLE `notification` DISABLE KEYS */;
/*!40000 ALTER TABLE `notification` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `notification_recipient`
--

DROP TABLE IF EXISTS `notification_recipient`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `notification_recipient` (
  `nr_notification_id` uuid NOT NULL,
  `nr_account_id` uuid NOT NULL,
  PRIMARY KEY (`nr_notification_id`,`nr_account_id`),
  KEY `fk_notification_recipient_account` (`nr_account_id`),
  CONSTRAINT `fk_notification_recipient_account` FOREIGN KEY (`nr_account_id`) REFERENCES `account` (`account_id`),
  CONSTRAINT `fk_notification_recipient_notification` FOREIGN KEY (`nr_notification_id`) REFERENCES `notification` (`notification_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notification_recipient`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `notification_recipient` WRITE;
/*!40000 ALTER TABLE `notification_recipient` DISABLE KEYS */;
/*!40000 ALTER TABLE `notification_recipient` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `permission`
--

DROP TABLE IF EXISTS `permission`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `permission` (
  `permission_id` uuid NOT NULL DEFAULT uuid_v7(),
  `permission_code` varchar(50) NOT NULL,
  `permission_name` varchar(150) NOT NULL,
  `permission_description` varchar(300) DEFAULT NULL,
  `permission_is_active` tinyint(1) NOT NULL DEFAULT 1,
  `permission_created_at` timestamp NULL DEFAULT current_timestamp(),
  `permission_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`permission_id`),
  UNIQUE KEY `idx_permission_code` (`permission_code`),
  UNIQUE KEY `idx_permission_name` (`permission_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `permission`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `permission` WRITE;
/*!40000 ALTER TABLE `permission` DISABLE KEYS */;
INSERT INTO `permission` VALUES
('01a0c449-5b96-7804-8aa8-c0162ace207b','IDENTITY_READ_LIST','Identity read list','Read identity records.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-79b0-aff7-e9500f9ea626','IDENTITY_STATUS_UPDATE','Identity status update','Update identity status.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7a44-b1b8-45e6af5b14f0','IDENTITY_CREATE_EMPLOYEE','Identity create employee','Create employee identities.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7a5c-aa79-672bc18696da','IDENTITY_CREATE_INSPECTOR','Identity create inspector','Create inspector identities.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7aa0-833c-b26932e0e962','IDENTITY_PERMISSION_CREATE','Identity permission create','Create permissions.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7ab4-84f6-4f2cc24ecd4e','IDENTITY_PERMISSION_ASSIGN','Identity permission assign','Assign permissions.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7ad8-854d-37e0586c0050','IDENTITY_PERMISSION_UPDATE','Identity permission update','Update permissions.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7aec-ae83-291191ac98f0','IDENTITY_PERMISSION_REVOKE','Identity permission revoke','Revoke permissions.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7b90-80b5-fc9e6a4e25d2','IDENTITY_ROLE_CREATE','Identity role create','Create roles.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7ba4-9fc3-9d89e875cd61','IDENTITY_ROLE_ASSIGN','Identity role assign','Assign roles.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7bb8-be27-2852b30657aa','IDENTITY_ROLE_UPDATE','Identity role update','Update roles.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7c00-9e6f-8a8989a2484e','IDENTITY_ROLE_REVOKE','Identity role revoke','Revoke roles.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7c1c-b08d-9aeb5a98ba25','PREMISE_CREATE','Premise create','Create premises.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7c34-892d-d11473dfc6f4','PREMISE_UPDATE','Premise update','Update premises.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7c48-8b71-be5070dac221','PREMISE_BUSINESS_CREATE','Premise business create','Create premise business types.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7c5c-b14f-dae9e3522595','PREMISE_BUSINESS_ASSIGN','Premise business assign','Assign business types to premises.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7cc8-8987-748cc814c38b','PREMISE_BUSINESS_REVOKE','Premise business revoke','Revoke premise business types.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7ce0-b7fd-4e24e2c0bd52','PREMISE_WHITELIST_CREATE','Premise whitelist create','Create premise whitelist entries.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7d20-8a56-40e60ed2a5ef','PREMISE_WHITELIST_REVOKE','Premise whitelist revoke','Revoke premise whitelist entries.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7d38-bf8b-a0597d158555','PREMISE_LOCATION_CREATE','Premise location create','Create premise locations.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7d54-a5d9-a085e6ee37da','PREMISE_LOCATION_UPDATE','Premise location update','Update premise locations.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7d68-9651-b759ed449093','PREMISE_LOCATION_REVOKE','Premise location revoke','Revoke premise locations.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7d80-a732-6f11abf7808d','PREMISE_STATUS_UPDATE','Premise status update','Update premise status.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7da8-8e53-972086e182f0','PREMISE_MEDIA_DELETE_HARD','Premise media delete hard','Hard-delete premise media.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7dc4-abc9-6f4beb4617e1','PREMISE_RENT','Premise rent','Rent a premise.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7dd8-95b8-896040d85df3','PREMISE_RETURN','Premise return','Return a rented premise.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7dec-845a-7ccca9028275','PREMISE_READ_OWN_RENTED','Premise read own rented','Read an owned rented premise.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7e00-ae0d-b47e6e2a150b','CONTRACT_READ_LIST','Contract read list','Read contract lists.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7e14-a126-574bc0bb0cb6','CONTRACT_CREATE','Contract create','Create contracts.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7e2c-b22b-f6b462b9b086','CONTRACT_APPROVE','Contract approve','Approve contracts.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7e90-a39e-8fd89a36bf26','CONTRACT_UPDATE','Contract update','Update contracts.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7ea8-959e-6374afee25ef','CONTRACT_STATUS_UPDATE','Contract status update','Update contract status.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7ec0-a0ed-7969c2b1bfc2','CONTRACT_TERMINATE_DATE_UPDATE','Contract terminate_date update','Update contract termination dates.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7ed4-aa62-f228ad985a9d','CONTRACT_REGULATION_READ','Contract regulation read','Read contract regulations.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7ee8-ab10-a1079c2810ad','CONTRACT_REGULATION_CREATE','Contract regulation create','Create contract regulations.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7efc-85c4-4e20292986cb','CONTRACT_REGULATION_UPDATE','Contract regulation update','Update contract regulations.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7f24-8cc3-1153900a0501','CONTRACT_REGULATION_REVOKE','Contract regulation revoke','Revoke contract regulations.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7f3c-b644-2ff558ca0d63','CONTRACT_VIOLATION_READ','Contract violation read','Read contract violations.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7f50-a688-08efc4cfafdd','CONTRACT_VIOLATION_CREATE','Contract violation create','Create contract violations.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7f64-acb0-923726fbb83e','CONTRACT_VIOLATION_UPDATE','Contract violation update','Update contract violations.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7f7c-b457-0b9cda4d7f7d','CONTRACT_VIOLATION_STATUS_UPDATE','Contract violation status update','Update contract violation status.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b96-7f90-95f7-58bd4074b7fe','CONTRACT_SIGN','Contract sign','Sign an owned contract.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-7054-b922-cf7ef07fca89','CONTRACT_TERMINATE','Contract terminate','Terminate an owned contract.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-7104-93a2-06a441c789b6','CONTRACT_CANCEL','Contract cancel','Cancel an owned contract.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-711c-8d80-475e0f4b56ff','CONTRACT_READ_OWN','Contract read own','Read an owned contract.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-7138-afc0-13a4fe39fb7e','CONTRACT_REGULATION_READ_OWN','Contract regulation read own','Read regulations of an owned contract.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-7180-917d-537d5c1bc026','CONTRACT_VIOLATION_READ_OWN','Contract violation read own','Read violations of an owned contract.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-71c0-a299-76eab0a23a81','CONTRACT_VIOLATION_PAY','Contract violation pay','Pay an owned contract violation.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-71d8-bde1-8eae027ac136','CONTRACT_VIOLATION_APPEAL','Contract violation appeal','Appeal an owned contract violation.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-71ec-a1d0-e13fe08933ec','INVOICE_READ_LIST','Invoice read list','Read invoice lists.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-7200-bdec-0776a931e481','INVOICE_CREATE','Invoice create','Create invoices.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-7214-8ff1-b68b90fef541','INVOICE_UPDATE','Invoice update','Update invoices.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-7228-9ac7-64ce395fef77','INVOICE_STATUS_UPDATE','Invoice status update','Update invoice status.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-723c-9c01-f9828ed5f2e9','INVOICE_READ_OWN','Invoice read own','Read an owned invoice.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-7250-8ac2-79026a051ed3','INVOICE_PAY','Invoice pay','Pay an owned invoice.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-7264-843f-9432178db5b7','RECEIPT_READ_LIST','Receipt read list','Read receipt lists.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-727c-8b0f-7d2172b4e48a','RECEIPT_CREATE','Receipt create','Create receipts.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-7290-aae5-5f7355ff5b96','RECEIPT_READ_OWN','Receipt read own','Read an owned receipt.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-72a4-9ae1-0a2e2cf948cd','TICKET_READ_LIST','Ticket read list','Read ticket lists.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34'),
('01a0c449-5b97-72b8-9d66-8ab642a0374e','NOTIFICATION_DELETE_HARD','Notification delete hard','Hard-delete notifications.',1,'2026-09-21 14:05:37','2026-09-21 15:23:34');
/*!40000 ALTER TABLE `permission` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_unicode_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trigger_permission_name_immutable`
    BEFORE UPDATE
    ON `permission`
    FOR EACH ROW
BEGIN
    IF NOT (OLD.`permission_name` <=> NEW.`permission_name`) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Permission name cannot be changed';
    END IF;
END 
*/;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_unicode_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trigger_permission_code_immutable`
    BEFORE UPDATE
    ON `permission`
    FOR EACH ROW
BEGIN
    IF NOT (OLD.`permission_code` <=> NEW.`permission_code`) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Permission code cannot be changed';
    END IF;
END 
*/;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `premise`
--

DROP TABLE IF EXISTS `premise`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `premise` (
  `premise_id` uuid NOT NULL DEFAULT uuid_v7(),
  `premise_name` varchar(50) NOT NULL,
  `premise_location_id` uuid NOT NULL,
  `premise_status` enum('rented','available','maintenance') NOT NULL DEFAULT 'available',
  `premise_position` int(11) NOT NULL,
  `premise_floor` int(11) NOT NULL,
  `premise_area` varchar(10) NOT NULL DEFAULT '',
  `premise_description` varchar(100) DEFAULT NULL,
  `premise_created_at` timestamp NULL DEFAULT current_timestamp(),
  `premise_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`premise_id`),
  KEY `fk_premise_location` (`premise_location_id`),
  KEY `idx_premise_name` (`premise_name`(20)),
  CONSTRAINT `fk_premise_location` FOREIGN KEY (`premise_location_id`) REFERENCES `location` (`location_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `premise`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `premise` WRITE;
/*!40000 ALTER TABLE `premise` DISABLE KEYS */;
/*!40000 ALTER TABLE `premise` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `premise_business_type`
--

DROP TABLE IF EXISTS `premise_business_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `premise_business_type` (
  `pre_bt_premise_id` uuid NOT NULL,
  `pre_bt_business_type_id` uuid NOT NULL,
  `pre_bt_asigned_at` timestamp NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`pre_bt_premise_id`,`pre_bt_business_type_id`),
  KEY `fk_premise_business_type_business_type` (`pre_bt_business_type_id`),
  CONSTRAINT `fk_premise_business_type_business_type` FOREIGN KEY (`pre_bt_business_type_id`) REFERENCES `business_type` (`business_type_id`),
  CONSTRAINT `fk_premise_business_type_premise` FOREIGN KEY (`pre_bt_premise_id`) REFERENCES `premise` (`premise_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `premise_business_type`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `premise_business_type` WRITE;
/*!40000 ALTER TABLE `premise_business_type` DISABLE KEYS */;
/*!40000 ALTER TABLE `premise_business_type` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `premise_media`
--

DROP TABLE IF EXISTS `premise_media`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `premise_media` (
  `premise_media_id` int(11) NOT NULL AUTO_INCREMENT,
  `premise_media_premise_id` uuid NOT NULL,
  `premise_media_image_url` varchar(255) NOT NULL,
  `premise_media_created_at` timestamp NULL DEFAULT current_timestamp(),
  `premise_media_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`premise_media_id`),
  KEY `fk_premise_media_premise` (`premise_media_premise_id`),
  CONSTRAINT `fk_premise_media_premise` FOREIGN KEY (`premise_media_premise_id`) REFERENCES `premise` (`premise_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `premise_media`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `premise_media` WRITE;
/*!40000 ALTER TABLE `premise_media` DISABLE KEYS */;
/*!40000 ALTER TABLE `premise_media` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `product_business_type`
--

DROP TABLE IF EXISTS `product_business_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `product_business_type` (
  `pbt_product_id` uuid NOT NULL,
  `pbt_business_type_id` uuid NOT NULL,
  `pbt_assigned_at` timestamp NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`pbt_product_id`,`pbt_business_type_id`),
  KEY `fk_product_business_type_business_type` (`pbt_business_type_id`),
  CONSTRAINT `fk_product_business_type_business_type` FOREIGN KEY (`pbt_business_type_id`) REFERENCES `business_type` (`business_type_id`),
  CONSTRAINT `fk_product_business_type_product` FOREIGN KEY (`pbt_product_id`) REFERENCES `whitelist_product` (`whitelist_product_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `product_business_type`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `product_business_type` WRITE;
/*!40000 ALTER TABLE `product_business_type` DISABLE KEYS */;
/*!40000 ALTER TABLE `product_business_type` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `receipt`
--

DROP TABLE IF EXISTS `receipt`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `receipt` (
  `receipt_id` uuid NOT NULL DEFAULT uuid_v7(),
  `receipt_invoice_id` uuid NOT NULL,
  `receipt_created_by_account_id` uuid NOT NULL,
  `receipt_payment_method` enum('vnpay','bank','cash') NOT NULL DEFAULT 'cash',
  `receipt_amount` decimal(18,2) NOT NULL DEFAULT 0.00,
  `receipt_payment_date` timestamp NOT NULL DEFAULT current_timestamp(),
  `receipt_transaction_reference` varchar(100) DEFAULT NULL,
  `receipt_gateway_transaction_number` varchar(100) DEFAULT NULL,
  `receipt_transaction_info` varchar(250) DEFAULT NULL,
  `receipt_bank_name` varchar(100) DEFAULT NULL,
  `receipt_created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `receipt_updated_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`receipt_id`),
  UNIQUE KEY `uk_receipt_invoice_id` (`receipt_invoice_id`),
  KEY `idx_receipt_payment_method` (`receipt_payment_method`),
  KEY `idx_receipt_payment_date` (`receipt_payment_date`),
  KEY `idx_receipt_transaction_reference` (`receipt_transaction_reference`),
  KEY `idx_receipt_created_by_account_id` (`receipt_created_by_account_id`),
  CONSTRAINT `fk_receipt_created_by_account` FOREIGN KEY (`receipt_created_by_account_id`) REFERENCES `account` (`account_id`),
  CONSTRAINT `fk_receipt_invoice` FOREIGN KEY (`receipt_invoice_id`) REFERENCES `monthly_invoice` (`invoice_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `receipt`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `receipt` WRITE;
/*!40000 ALTER TABLE `receipt` DISABLE KEYS */;
/*!40000 ALTER TABLE `receipt` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `regulation`
--

DROP TABLE IF EXISTS `regulation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `regulation` (
  `regulation_id` uuid NOT NULL DEFAULT uuid_v7(),
  `regulation_name` varchar(50) NOT NULL,
  `regulation_description` varchar(255) DEFAULT NULL,
  `regulation_fine_amount` decimal(18,2) DEFAULT NULL,
  `regulation_is_active` tinyint(1) NOT NULL DEFAULT 1,
  `regulation_created_at` timestamp NULL DEFAULT current_timestamp(),
  `regulation_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`regulation_id`),
  KEY `idx_regulation_name` (`regulation_name`(20))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `regulation`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `regulation` WRITE;
/*!40000 ALTER TABLE `regulation` DISABLE KEYS */;
/*!40000 ALTER TABLE `regulation` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `rented_premise`
--

DROP TABLE IF EXISTS `rented_premise`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `rented_premise` (
  `rented_premise_contract_id` uuid NOT NULL,
  `rented_premise_premise_id` uuid NOT NULL,
  PRIMARY KEY (`rented_premise_premise_id`,`rented_premise_contract_id`),
  KEY `fk_rented_premise_contract` (`rented_premise_contract_id`),
  CONSTRAINT `fk_rented_premise_contract` FOREIGN KEY (`rented_premise_contract_id`) REFERENCES `contract` (`contract_id`),
  CONSTRAINT `fk_rented_premise_premise` FOREIGN KEY (`rented_premise_premise_id`) REFERENCES `premise` (`premise_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rented_premise`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `rented_premise` WRITE;
/*!40000 ALTER TABLE `rented_premise` DISABLE KEYS */;
/*!40000 ALTER TABLE `rented_premise` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `role` (
  `role_id` uuid NOT NULL DEFAULT uuid_v7(),
  `role_code` varchar(50) NOT NULL,
  `role_name` varchar(150) NOT NULL,
  `role_description` varchar(300) DEFAULT NULL,
  `role_is_active` tinyint(1) NOT NULL DEFAULT 1,
  `role_created_at` timestamp NULL DEFAULT current_timestamp(),
  `role_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`role_id`),
  UNIQUE KEY `idx_role_code` (`role_code`),
  UNIQUE KEY `idx_role_name` (`role_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `role` WRITE;
/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role` VALUES
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','CUSTOMER','Customer','Customer role for browsing products, managing cart items, and making purchases.',1,'2026-09-21 13:34:17','2026-09-21 13:34:17'),
('01a0c42c-af6b-79bc-95f3-47764309c0e6','EMPLOYEE','Employee','Employee role for create contract, reading role information and own permissions.',1,'2026-09-21 13:34:17','2026-09-21 13:34:17'),
('01a0c42c-af6b-7a1c-b6b1-61af0becd83b','INSPECTOR','Inspector','Inspector role for reviewing and validating information.',1,'2026-09-21 13:34:17','2026-09-21 13:34:17'),
('01a0c42c-af6b-7a48-a097-8d0537814615','MANAGER','Manager','Manager role for product, customer, order, role and permission management, plus statistics access.',1,'2026-09-21 13:34:17','2026-09-21 13:34:17'),
('01a0c42c-af6b-7a6c-9560-c0f3e79af41e','ADMIN','Admin','System administrator with full administrative permissions. Customer purchase and employee selling capabilities are not assigned as explicit permissions.',1,'2026-09-21 13:34:17','2026-09-21 13:34:17');
/*!40000 ALTER TABLE `role` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_unicode_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trigger_role_code_immutable`
    BEFORE UPDATE
    ON `role`
    FOR EACH ROW
BEGIN
    IF NOT (OLD.`role_code` <=> NEW.`role_code`) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Role code cannot be changed';
    END IF;
END 
*/;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `role_permission`
--

DROP TABLE IF EXISTS `role_permission`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `role_permission` (
  `role_id` uuid NOT NULL,
  `permission_id` uuid NOT NULL,
  `assigned_at` timestamp NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`role_id`,`permission_id`),
  KEY `idx_role_permission_permission_id` (`permission_id`),
  CONSTRAINT `fk_role_permission_permission` FOREIGN KEY (`permission_id`) REFERENCES `permission` (`permission_id`),
  CONSTRAINT `fk_role_permission_role` FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role_permission`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `role_permission` WRITE;
/*!40000 ALTER TABLE `role_permission` DISABLE KEYS */;
INSERT INTO `role_permission` VALUES
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b96-7dc4-abc9-6f4beb4617e1','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b96-7dd8-95b8-896040d85df3','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b96-7dec-845a-7ccca9028275','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b96-7f90-95f7-58bd4074b7fe','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b97-7054-b922-cf7ef07fca89','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b97-7104-93a2-06a441c789b6','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b97-711c-8d80-475e0f4b56ff','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b97-7138-afc0-13a4fe39fb7e','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b97-7180-917d-537d5c1bc026','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b97-71c0-a299-76eab0a23a81','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b97-71d8-bde1-8eae027ac136','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b97-723c-9c01-f9828ed5f2e9','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b97-7250-8ac2-79026a051ed3','2026-09-21 14:05:37'),
('01a0c42c-af6b-7740-bfe7-619e643d7b6b','01a0c449-5b97-7290-aae5-5f7355ff5b96','2026-09-21 14:05:37'),
('01a0c42c-af6b-79bc-95f3-47764309c0e6','01a0c449-5b96-7d80-a732-6f11abf7808d','2026-09-21 14:05:37'),
('01a0c42c-af6b-79bc-95f3-47764309c0e6','01a0c449-5b96-7e14-a126-574bc0bb0cb6','2026-09-21 14:05:37'),
('01a0c42c-af6b-79bc-95f3-47764309c0e6','01a0c449-5b96-7ea8-959e-6374afee25ef','2026-09-21 14:05:37'),
('01a0c42c-af6b-79bc-95f3-47764309c0e6','01a0c449-5b96-7ec0-a0ed-7969c2b1bfc2','2026-09-21 14:05:37'),
('01a0c42c-af6b-79bc-95f3-47764309c0e6','01a0c449-5b96-7f7c-b457-0b9cda4d7f7d','2026-09-21 14:05:37'),
('01a0c42c-af6b-79bc-95f3-47764309c0e6','01a0c449-5b97-71ec-a1d0-e13fe08933ec','2026-09-21 14:05:37'),
('01a0c42c-af6b-79bc-95f3-47764309c0e6','01a0c449-5b97-7228-9ac7-64ce395fef77','2026-09-21 14:05:37'),
('01a0c42c-af6b-79bc-95f3-47764309c0e6','01a0c449-5b97-7264-843f-9432178db5b7','2026-09-21 14:05:37'),
('01a0c42c-af6b-79bc-95f3-47764309c0e6','01a0c449-5b97-727c-8b0f-7d2172b4e48a','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a1c-b6b1-61af0becd83b','01a0c449-5b96-7e00-ae0d-b47e6e2a150b','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a1c-b6b1-61af0becd83b','01a0c449-5b96-7ed4-aa62-f228ad985a9d','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a1c-b6b1-61af0becd83b','01a0c449-5b96-7f3c-b644-2ff558ca0d63','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7804-8aa8-c0162ace207b','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-79b0-aff7-e9500f9ea626','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7a44-b1b8-45e6af5b14f0','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7a5c-aa79-672bc18696da','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7aa0-833c-b26932e0e962','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7ab4-84f6-4f2cc24ecd4e','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7ad8-854d-37e0586c0050','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7aec-ae83-291191ac98f0','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7b90-80b5-fc9e6a4e25d2','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7ba4-9fc3-9d89e875cd61','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7bb8-be27-2852b30657aa','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7c00-9e6f-8a8989a2484e','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7c1c-b08d-9aeb5a98ba25','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7c34-892d-d11473dfc6f4','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7c48-8b71-be5070dac221','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7c5c-b14f-dae9e3522595','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7cc8-8987-748cc814c38b','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7ce0-b7fd-4e24e2c0bd52','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7d20-8a56-40e60ed2a5ef','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7d38-bf8b-a0597d158555','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7d54-a5d9-a085e6ee37da','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7d68-9651-b759ed449093','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7d80-a732-6f11abf7808d','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7da8-8e53-972086e182f0','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7e00-ae0d-b47e6e2a150b','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7e2c-b22b-f6b462b9b086','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7e90-a39e-8fd89a36bf26','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7ea8-959e-6374afee25ef','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7ec0-a0ed-7969c2b1bfc2','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7ee8-ab10-a1079c2810ad','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7efc-85c4-4e20292986cb','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7f24-8cc3-1153900a0501','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7f3c-b644-2ff558ca0d63','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7f50-a688-08efc4cfafdd','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b96-7f64-acb0-923726fbb83e','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b97-71ec-a1d0-e13fe08933ec','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b97-7200-bdec-0776a931e481','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b97-7214-8ff1-b68b90fef541','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b97-7264-843f-9432178db5b7','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b97-72a4-9ae1-0a2e2cf948cd','2026-09-21 14:05:37'),
('01a0c42c-af6b-7a48-a097-8d0537814615','01a0c449-5b97-72b8-9d66-8ab642a0374e','2026-09-21 14:05:37');
/*!40000 ALTER TABLE `role_permission` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `ticket`
--

DROP TABLE IF EXISTS `ticket`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `ticket` (
  `ticket_id` uuid NOT NULL DEFAULT uuid_v7(),
  `ticket_account_id` uuid NOT NULL,
  `ticket_content` varchar(255) NOT NULL,
  `ticket_type` enum('review','complaint','feedback') NOT NULL DEFAULT 'feedback',
  `ticket_is_resolved` tinyint(1) NOT NULL DEFAULT 0,
  `ticket_created_at` timestamp NULL DEFAULT current_timestamp(),
  `ticket_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`ticket_id`),
  KEY `fk_ticket_account` (`ticket_account_id`),
  CONSTRAINT `fk_ticket_account` FOREIGN KEY (`ticket_account_id`) REFERENCES `account` (`account_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `ticket` WRITE;
/*!40000 ALTER TABLE `ticket` DISABLE KEYS */;
/*!40000 ALTER TABLE `ticket` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `ticket_media`
--

DROP TABLE IF EXISTS `ticket_media`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `ticket_media` (
  `ticket_media_id` int(11) NOT NULL AUTO_INCREMENT,
  `ticket_media_ticket_id` uuid NOT NULL,
  `ticket_media_image_url` varchar(255) NOT NULL,
  PRIMARY KEY (`ticket_media_id`),
  KEY `fk_ticket_media_ticket` (`ticket_media_ticket_id`),
  CONSTRAINT `fk_ticket_media_ticket` FOREIGN KEY (`ticket_media_ticket_id`) REFERENCES `ticket` (`ticket_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket_media`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `ticket_media` WRITE;
/*!40000 ALTER TABLE `ticket_media` DISABLE KEYS */;
/*!40000 ALTER TABLE `ticket_media` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `user_profile`
--

DROP TABLE IF EXISTS `user_profile`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_profile` (
  `user_profile_id` varchar(12) NOT NULL,
  `user_profile_account_id` uuid NOT NULL,
  `user_profile_first_name` varchar(30) DEFAULT NULL,
  `user_profile_last_name` varchar(30) DEFAULT NULL,
  `user_profile_date_of_birth` date DEFAULT NULL,
  `user_profile_gender` enum('male','female','unspecified') NOT NULL DEFAULT 'unspecified',
  `user_profile_phone_number` varchar(10) DEFAULT NULL,
  `user_profile_address` varchar(255) NOT NULL DEFAULT '',
  `user_profile_avatar_url` varchar(255) DEFAULT NULL,
  `user_profile_created_at` timestamp NULL DEFAULT current_timestamp(),
  `user_profile_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`user_profile_id`),
  UNIQUE KEY `idx_user_profile_account_id` (`user_profile_account_id`),
  UNIQUE KEY `user_profile_cccd` (`user_profile_id`),
  KEY `idx_user_profile_phone_number` (`user_profile_phone_number`),
  KEY `idx_user_profile_date_of_birth` (`user_profile_date_of_birth`),
  CONSTRAINT `fk_user_profile_account` FOREIGN KEY (`user_profile_account_id`) REFERENCES `account` (`account_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_profile`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `user_profile` WRITE;
/*!40000 ALTER TABLE `user_profile` DISABLE KEYS */;
INSERT INTO `user_profile` VALUES
('000000000001','01a051c7-348f-7dc8-9f4a-d9efcd9171e3','John','Doe','1990-01-01','male','0984242001','',NULL,'2026-08-31 14:45:10','2026-09-15 02:24:34'),
('000000000002','01a051c7-348f-7f88-9c23-9c1c8b0ea021','Jane','Manager','2000-01-12','female','0843220324','',NULL,'2026-08-31 14:45:10','2026-09-15 02:24:34'),
('000000000003','01a051c7-3490-701c-ac2a-28bc50582f30','Lilith','','2003-11-12','female','0843220326','',NULL,'2026-08-31 14:45:10','2026-09-15 02:24:34'),
('000000000004','01a051c7-3490-702c-944f-b6b57ab43f9f','Bob','Smith','1995-05-20','male','0843210324','',NULL,'2026-08-31 14:45:10','2026-09-15 02:24:34'),
('000000000005','01a051c7-3490-7034-b170-58e933015301','Alice','Smith','2005-05-20','female','0843220024','',NULL,'2026-08-31 14:45:10','2026-09-15 02:24:34'),
('000000000006','01a051c7-3490-7040-84fe-d385f1e01893','Tom','Halland','2005-01-20','male','0841210324','',NULL,'2026-08-31 14:45:10','2026-09-15 02:24:34'),
('000000000007','01a051c7-3490-7048-a9eb-cea8b71457d4','Thien','Lang','2005-01-20','male','0843120314','',NULL,'2026-08-31 14:45:10','2026-09-15 02:24:34'),
('000000000008','01a052fc-efb9-7ef1-b57f-029e779396de','Dien','Vy','2002-01-20','unspecified',NULL,'',NULL,'2026-08-31 14:45:10','2026-09-15 02:24:34'),
('000000000009','01a0b2ee-2899-7eb8-bb16-4dc3c6387a2d','Thien','Lang','2026-09-18','male','0843222111','449HL2','string','2026-09-18 06:27:40','2026-09-18 06:27:40');
/*!40000 ALTER TABLE `user_profile` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;

--
-- Table structure for table `whitelist_product`
--

DROP TABLE IF EXISTS `whitelist_product`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `whitelist_product` (
  `whitelist_product_id` uuid NOT NULL DEFAULT uuid_v7(),
  `whitelist_product_name` varchar(100) NOT NULL,
  `whitelist_product_description` varchar(255) DEFAULT NULL,
  `whitelist_product_created_at` timestamp NULL DEFAULT current_timestamp(),
  `whitelist_product_updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`whitelist_product_id`),
  KEY `idx_whitelist_product_name` (`whitelist_product_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `whitelist_product`
--

SET @OLD_AUTOCOMMIT=@@AUTOCOMMIT, @@AUTOCOMMIT=0;
LOCK TABLES `whitelist_product` WRITE;
/*!40000 ALTER TABLE `whitelist_product` DISABLE KEYS */;
/*!40000 ALTER TABLE `whitelist_product` ENABLE KEYS */;
UNLOCK TABLES;
COMMIT;
SET AUTOCOMMIT=@OLD_AUTOCOMMIT;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*M!100616 SET NOTE_VERBOSITY=@OLD_NOTE_VERBOSITY */;

-- Dump completed on 2026-10-04 11:20:20
