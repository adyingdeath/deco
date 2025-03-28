package com.adyingdeath.deco.datapack.advancement;

import com.google.gson.JsonObject;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class AdvancementManager {
    private final Map<String, JsonObject> advancements;
    public AdvancementManager() {
        this.advancements = new HashMap<>();
    }

    public void addAdvancement(String location, JsonObject ad) {
        this.advancements.put(location, ad);
    }
}
