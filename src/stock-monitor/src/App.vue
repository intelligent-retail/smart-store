<template>
  <div id="app">
    <Header />
    <router-view class="container" />
    <Footer />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import Header from "@/components/Header.vue";
import Footer from "@/components/Footer.vue";

export default Vue.extend({
  components: {
    Header,
    Footer
  }
});
</script>
