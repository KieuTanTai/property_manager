package com.example.contract.Models.Contract;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.UUID;

import org.hibernate.annotations.CreationTimestamp;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "contract_violation")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class ContractViolationModel {
    @Id 
    @Column(name = "contract_violation_id", nullable = false)
    private UUID violationId;

    @Column(name = "contract_id", nullable = false)
    private UUID contractId;

    @Column(name = "violation_content", nullable = false, length = 150)
    private String violationContent;

    @Column(name = "violation_penalty_amount", precision = 18, scale = 2)
    private BigDecimal violationPenaltyAmount;

    @CreationTimestamp
    @Column(name = "violation_date", nullable = false, insertable=false, updatable=false)
    private LocalDateTime violationDate;

    @Column(name = "violation_due_date", nullable = false)
    private LocalDateTime violationDueDate;

    @Column(name = "violation_is_resolved", nullable = false)
    private Boolean violationIsResolved;
}
