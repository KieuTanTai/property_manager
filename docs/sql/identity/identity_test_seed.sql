-- Permission and role-permission seed data.
-- Roles are expected to exist with the IDs listed below.
--
-- CUSTOMER  01a0c42c-af6b-7740-bfe7-619e643d7b6b
-- EMPLOYEE  01a0c42c-af6b-79bc-95f3-47764309c0e6
-- INSPECTOR 01a0c42c-af6b-7a1c-b6b1-61af0becd83b
-- MANAGER   01a0c42c-af6b-7a48-a097-8d0537814615
-- ADMIN     01a0c42c-af6b-7a6c-9560-c0f3e79af41e

USE `ms_identity_test`;
START TRANSACTION;

INSERT INTO `role`
    (`role_code`, `role_name`, `role_description`, `role_is_active`)
VALUES
    ('CUSTOMER',
     'Customer',
     'Customer role for browsing products, managing cart items, and making purchases.',
     TRUE),
    ('EMPLOYEE',
     'Employee',
     'Employee role for create contract, reading role information and own permissions.',
     TRUE),
    ('INSPECTOR',
     'Inspector',
     'Inspector role for reviewing and validating information.',
     TRUE),
    ('MANAGER',
     'Manager',
     'Manager role for product, customer, order, role and permission management, plus statistics access.',
     TRUE),
    ('ADMIN',
     'Admin',
     'System administrator with full administrative permissions. Customer purchase and employee selling capabilities are not assigned as explicit permissions.',
     TRUE);

INSERT INTO `permission`
    (`permission_code`, `permission_name`, `permission_description`, `permission_is_active`)
VALUES
    ('IDENTITY_READ_LIST', 'Identity Read List', 'Read identity records.', TRUE),
    ('IDENTITY_STATUS_UPDATE', 'Identity Status Update', 'Update identity status.', TRUE),
    ('IDENTITY_CREATE_EMPLOYEE', 'Identity Create Employee', 'Create employee identities.', TRUE),
    ('IDENTITY_CREATE_INSPECTOR', 'Identity Create Inspector', 'Create inspector identities.', TRUE),
    ('IDENTITY_PERMISSION_CREATE', 'Identity Permission Create', 'Create permissions.', TRUE),
    ('IDENTITY_PERMISSION_ASSIGN', 'Identity Permission Assign', 'Assign permissions.', TRUE),
    ('IDENTITY_PERMISSION_UPDATE', 'Identity Permission Update', 'Update permissions.', TRUE),
    ('IDENTITY_PERMISSION_REVOKE', 'Identity Permission Revoke', 'Revoke permissions.', TRUE),
    ('IDENTITY_ROLE_CREATE', 'Identity Role Create', 'Create roles.', TRUE),
    ('IDENTITY_ROLE_ASSIGN', 'Identity Role Assign', 'Assign roles.', TRUE),
    ('IDENTITY_ROLE_UPDATE', 'Identity Role Update', 'Update roles.', TRUE),
    ('IDENTITY_ROLE_REVOKE', 'Identity Role Revoke', 'Revoke roles.', TRUE),

    ('PREMISE_CREATE', 'Premise Create', 'Create premises.', TRUE),
    ('PREMISE_UPDATE', 'Premise Update', 'Update premises.', TRUE),
    ('PREMISE_BUSINESS_CREATE', 'Premise Business Create', 'Create premise business types.', TRUE),
    ('PREMISE_BUSINESS_ASSIGN', 'Premise Business Assign', 'Assign business types to premises.', TRUE),
    ('PREMISE_BUSINESS_REVOKE', 'Premise Business Revoke', 'Revoke premise business types.', TRUE),
    ('PREMISE_WHITELIST_CREATE', 'Premise Whitelist Create', 'Create premise whitelist entries.', TRUE),
    ('PREMISE_WHITELIST_REVOKE', 'Premise Whitelist Revoke', 'Revoke premise whitelist entries.', TRUE),
    ('PREMISE_LOCATION_CREATE', 'Premise Location Create', 'Create premise locations.', TRUE),
    ('PREMISE_LOCATION_UPDATE', 'Premise Location Update', 'Update premise locations.', TRUE),
    ('PREMISE_LOCATION_REVOKE', 'Premise Location Revoke', 'Revoke premise locations.', TRUE),
    ('PREMISE_STATUS_UPDATE', 'Premise Status Update', 'Update premise status.', TRUE),
    ('PREMISE_MEDIA_DELETE_HARD', 'Premise Media Delete Hard', 'Hard-delete premise media.', TRUE),
    ('PREMISE_RENT', 'Premise Rent', 'Rent a premise.', TRUE),
    ('PREMISE_RETURN', 'Premise Return', 'Return a rented premise.', TRUE),
    ('PREMISE_READ_OWN_RENTED', 'Premise Read Own Rented', 'Read an owned rented premise.', TRUE),

    ('CONTRACT_READ_LIST', 'Contract Read List', 'Read contract lists.', TRUE),
    ('CONTRACT_CREATE', 'Contract Create', 'Create contracts.', TRUE),
    ('CONTRACT_APPROVE', 'Contract Approve', 'Approve contracts.', TRUE),
    ('CONTRACT_UPDATE', 'Contract Update', 'Update contracts.', TRUE),
    ('CONTRACT_STATUS_UPDATE', 'Contract Status Update', 'Update contract status.', TRUE),
    ('CONTRACT_TERMINATE_DATE_UPDATE', 'Contract Terminate Date Update', 'Update contract termination dates.', TRUE),
    ('CONTRACT_REGULATION_READ', 'Contract Regulation Read', 'Read contract regulations.', TRUE),
    ('CONTRACT_REGULATION_CREATE', 'Contract Regulation Create', 'Create contract regulations.', TRUE),
    ('CONTRACT_REGULATION_UPDATE', 'Contract Regulation Update', 'Update contract regulations.', TRUE),
    ('CONTRACT_REGULATION_REVOKE', 'Contract Regulation Revoke', 'Revoke contract regulations.', TRUE),
    ('CONTRACT_VIOLATION_READ', 'Contract Violation Read', 'Read contract violations.', TRUE),
    ('CONTRACT_VIOLATION_CREATE', 'Contract Violation Create', 'Create contract violations.', TRUE),
    ('CONTRACT_VIOLATION_UPDATE', 'Contract Violation Update', 'Update contract violations.', TRUE),
    ('CONTRACT_VIOLATION_STATUS_UPDATE', 'Contract Violation Status Update', 'Update contract violation status.', TRUE),
    ('CONTRACT_SIGN', 'Contract Sign', 'Sign an owned contract.', TRUE),
    ('CONTRACT_TERMINATE', 'Contract Terminate', 'Terminate an owned contract.', TRUE),
    ('CONTRACT_CANCEL', 'Contract Cancel', 'Cancel an owned contract.', TRUE),
    ('CONTRACT_READ_OWN', 'Contract Read Own', 'Read an owned contract.', TRUE),
    ('CONTRACT_REGULATION_READ_OWN', 'Contract Regulation Read Own', 'Read regulations of an owned contract.', TRUE),
    ('CONTRACT_VIOLATION_READ_OWN', 'Contract Violation Read Own', 'Read violations of an owned contract.', TRUE),
    ('CONTRACT_VIOLATION_PAY', 'Contract Violation Pay', 'Pay an owned contract violation.', TRUE),
    ('CONTRACT_VIOLATION_APPEAL', 'Contract Violation Appeal', 'Appeal an owned contract violation.', TRUE),

    ('INVOICE_READ_LIST', 'Invoice Read List', 'Read invoice lists.', TRUE),
    ('INVOICE_CREATE', 'Invoice Create', 'Create invoices.', TRUE),
    ('INVOICE_UPDATE', 'Invoice Update', 'Update invoices.', TRUE),
    ('INVOICE_STATUS_UPDATE', 'Invoice Status Update', 'Update invoice status.', TRUE),
    ('INVOICE_READ_OWN', 'Invoice Read Own', 'Read an owned invoice.', TRUE),
    ('INVOICE_PAY', 'Invoice Pay', 'Pay an owned invoice.', TRUE),

    ('RECEIPT_READ_LIST', 'Receipt Read List', 'Read receipt lists.', TRUE),
    ('RECEIPT_CREATE', 'Receipt Create', 'Create receipts.', TRUE),
    ('RECEIPT_READ_OWN', 'Receipt Read Own', 'Read an owned receipt.', TRUE),

    ('TICKET_READ_LIST', 'Ticket Read List', 'Read ticket lists.', TRUE),
    ('NOTIFICATION_DELETE_HARD', 'Notification Delete Hard', 'Hard-delete notifications.', TRUE);

-- EMPLOYEE
INSERT INTO `role_permission` (`role_id`, `permission_id`)
SELECT r.`role_id`, p.`permission_id`
FROM `role` r
CROSS JOIN `permission` p
WHERE r.`role_code` = 'EMPLOYEE'
  AND p.`permission_code` IN (
    'PREMISE_STATUS_UPDATE',
    'CONTRACT_CREATE',
    'CONTRACT_STATUS_UPDATE',
    'CONTRACT_TERMINATE_DATE_UPDATE',
    'CONTRACT_VIOLATION_STATUS_UPDATE',
    'INVOICE_READ_LIST',
    'INVOICE_STATUS_UPDATE',
    'RECEIPT_CREATE',
    'RECEIPT_READ_LIST'
);

-- INSPECTOR
INSERT INTO `role_permission` (`role_id`, `permission_id`)
SELECT r.`role_id`, p.`permission_id`
FROM `role` r
CROSS JOIN `permission` p
WHERE r.`role_code` = 'INSPECTOR'
  AND p.`permission_code` IN (
    'CONTRACT_READ_LIST',
    'CONTRACT_REGULATION_READ',
    'CONTRACT_VIOLATION_READ'
);

-- MANAGER
INSERT INTO `role_permission` (`role_id`, `permission_id`)
SELECT r.`role_id`, p.`permission_id`
FROM `role` r
CROSS JOIN `permission` p
WHERE r.`role_code` = 'MANAGER'
  AND p.`permission_code` IN (
    'IDENTITY_READ_LIST',
    'IDENTITY_STATUS_UPDATE',
    'IDENTITY_CREATE_EMPLOYEE',
    'IDENTITY_CREATE_INSPECTOR',
    'IDENTITY_PERMISSION_CREATE',
    'IDENTITY_PERMISSION_ASSIGN',
    'IDENTITY_PERMISSION_UPDATE',
    'IDENTITY_PERMISSION_REVOKE',
    'IDENTITY_ROLE_CREATE',
    'IDENTITY_ROLE_ASSIGN',
    'IDENTITY_ROLE_UPDATE',
    'IDENTITY_ROLE_REVOKE',
    'PREMISE_CREATE',
    'PREMISE_UPDATE',
    'PREMISE_BUSINESS_CREATE',
    'PREMISE_BUSINESS_ASSIGN',
    'PREMISE_BUSINESS_REVOKE',
    'PREMISE_WHITELIST_CREATE',
    'PREMISE_WHITELIST_REVOKE',
    'PREMISE_LOCATION_CREATE',
    'PREMISE_LOCATION_UPDATE',
    'PREMISE_LOCATION_REVOKE',
    'PREMISE_STATUS_UPDATE',
    'PREMISE_MEDIA_DELETE_HARD',
    'CONTRACT_READ_LIST',
    'CONTRACT_APPROVE',
    'CONTRACT_UPDATE',
    'CONTRACT_STATUS_UPDATE',
    'CONTRACT_TERMINATE_DATE_UPDATE',
    'CONTRACT_REGULATION_CREATE',
    'CONTRACT_REGULATION_UPDATE',
    'CONTRACT_REGULATION_REVOKE',
    'CONTRACT_VIOLATION_READ',
    'CONTRACT_VIOLATION_CREATE',
    'CONTRACT_VIOLATION_UPDATE',
    'INVOICE_READ_LIST',
    'INVOICE_CREATE',
    'INVOICE_UPDATE',
    'RECEIPT_READ_LIST',
    'TICKET_READ_LIST',
    'NOTIFICATION_DELETE_HARD'
);

-- CUSTOMER: dynamic permissions are assigned here because their policy
-- additionally checks ownership of the requested resource.
INSERT INTO `role_permission` (`role_id`, `permission_id`)
SELECT r.`role_id`, p.`permission_id`
FROM `role` r
CROSS JOIN `permission` p
WHERE r.`role_code` = 'CUSTOMER'
  AND p.`permission_code` IN (
    'PREMISE_RENT',
    'PREMISE_RETURN',
    'PREMISE_READ_OWN_RENTED',
    'CONTRACT_SIGN',
    'CONTRACT_TERMINATE',
    'CONTRACT_CANCEL',
    'CONTRACT_READ_OWN',
    'CONTRACT_REGULATION_READ_OWN',
    'CONTRACT_VIOLATION_READ_OWN',
    'CONTRACT_VIOLATION_PAY',
    'CONTRACT_VIOLATION_APPEAL',
    'INVOICE_READ_OWN',
    'INVOICE_PAY',
    'RECEIPT_READ_OWN'
);

COMMIT;

select * from account;