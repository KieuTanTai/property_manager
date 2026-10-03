package com.example.contract.Models.Invoice;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.UUID;

import com.example.Shared.Enum.EReceiptPaymentMethod;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EnumType;
import jakarta.persistence.Enumerated;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.UniqueConstraint;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "receipt",
        uniqueConstraints = {
            @UniqueConstraint(
                    name = "uk_receipt_invoice_id",
                    columnNames = "receipt_invoice_id"
            )
        })
@Getter
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class ReceiptModel {
    @Id 
    @Column(name = "receipt_id", nullable = false)
    private UUID receiptId;

    @Column(name = "receipt_invoice_id", nullable = false)
    private UUID receiptInvoiceId;

    @Column(name = "receipt_created_by_account_id", nullable = false)
    private UUID receiptCreatedByAccountId;

    @Enumerated(EnumType.STRING)
    @Column(name = "receipt_payment_method", nullable = false)
    private EReceiptPaymentMethod receiptPaymentMethod;

    @Column(name = "receipt_amount", nullable = false, precision = 18, scale = 2)
    private BigDecimal receiptAmount;

    @Column(name = "receipt_payment_date", nullable = false)
    private LocalDateTime receiptPaymentDate;

    @Column(name = "receipt_transaction_reference", length = 100)
    private String receiptTransactionReference;

    @Column(name = "receipt_gateway_transaction_number", length = 100)
    private String receiptGatewayTransactionNumber;

    @Column(name = "receipt_transaction_info", length = 255)
    private String receiptTransactionInfo;

    @Column(name = "receipt_bank_name", length = 100)
    private String receiptBankName;

    @Column(name = "receipt_created_at")
    private LocalDateTime receiptCreatedAt;

    @Column (name = "receipt_updated_at")
    private LocalDateTime receiptUpdatedAt;
}
