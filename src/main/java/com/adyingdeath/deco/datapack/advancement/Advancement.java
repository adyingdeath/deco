package com.adyingdeath.deco.datapack.advancement;

import com.google.gson.JsonObject;

import java.util.HashMap;
import java.util.Map;

public class Advancement {
    private final Map<String, JsonObject> advancements;
    public Advancement() {
        this.advancements = new HashMap<>();
    }

    public void addAdvancement(String location, JsonObject ad) {
        this.advancements.put(location, ad);
    }

    public Map<String, JsonObject> getAdvancements() {
        return this.advancements;
    }
}
