package com.example.contract.Models.Contract;

import java.io.Serializable;
import java.time.LocalDateTime;
import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;
import jakarta.persistence.EmbeddedId;
import jakarta.persistence.Entity;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.EqualsAndHashCode;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "contract_regulation")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class ContractRegulationModel implements Serializable {
    @EmbeddedId 
    private ContractRegulationId id;

    @Column(name = "assigned_at")
    private LocalDateTime assignedAt;

    @Embeddable 
    @Getter 
    @Setter 
    @NoArgsConstructor 
    @AllArgsConstructor 
    @EqualsAndHashCode 
    public static class ContractRegulationId implements Serializable {
        @Column(name = "regulation_id", nullable = false)
        private UUID contractRegulationType;
        @Column(name = "contract_id", nullable = false)
        private UUID contractId;
    }
}
