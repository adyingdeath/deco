package com.adyingdeath.deco.datapack.advancement;

import com.google.gson.JsonObject;

import java.util.ArrayList;
import java.util.List;

public class AdvancementManager {
    private final List<JsonObject> advancements;
    public AdvancementManager() {
        this.advancements = new ArrayList<>();
    }

    public void addAdvancement(JsonObject ad) {
        this.advancements.add(ad);
    }

    public List<JsonObject> getAdvancements() {
        return advancements;
    }
}
