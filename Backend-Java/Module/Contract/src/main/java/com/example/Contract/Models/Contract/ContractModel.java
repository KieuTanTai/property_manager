package com.example.contract.Models.Contract;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.UUID;

import com.example.Shared.Enum.EContractStatus;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EnumType;
import jakarta.persistence.Enumerated;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "contract")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class ContractModel {
    @Id 
    @Column(name = "contract_id", nullable = false)
    private UUID contractId;

    @Column(name = "contract_account_id", nullable = false)
    private UUID contractAccountId;

    @Column(name = "contract_deposit", nullable = false, precision = 18, scale = 2)
    private BigDecimal contractDeposit;

    @Column(name = "contract_rental_price", nullable = false, precision = 18, scale = 2)
    private BigDecimal contractRentalPrice;

    @Column(name = "contract_premise_return_date", nullable = false)
    private LocalDateTime contractPremiseReturnDate;

    @Enumerated(EnumType.STRING)
    @Column(name = "contract_status", nullable = false)
    private EContractStatus contractStatus;

    @Column(name = "contract_termination_date")
    private LocalDateTime contractTerminationDate;

    @Column(name = "contract_created_at")
    private LocalDateTime contractCreatedAt;

    @Column(name = "contract_updated_at")
    private LocalDateTime contractUpdatedAt;
}
