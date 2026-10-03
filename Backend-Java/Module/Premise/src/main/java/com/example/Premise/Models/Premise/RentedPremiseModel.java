package com.example.Premise.Models.Premise;

import java.io.Serializable;
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
@Table (name = "rented_premise")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class RentedPremiseModel implements Serializable{
    @EmbeddedId 
    private RentedPremiseId id;

    @Embeddable 
    @Getter 
    @Setter 
    @NoArgsConstructor
    @AllArgsConstructor
    @EqualsAndHashCode 
    public static class RentedPremiseId implements Serializable {
        @Column(name = "rented_premise_contract_id", nullable = false)
        private UUID contractId;

        @Column(name = "rented_premise_premise_id", nullable = false)
        private UUID premiseId;
    }
}
