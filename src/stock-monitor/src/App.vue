<template>
  <div id="app">
    <Header />
    <router-view class="container" />
    <Footer />
  </div>
</template>

<script lang="ts">
import Header from '@/components/Header.vue'
import Footer from '@/components/Footer.vue'

export default {
  components: {
    Header,
    Footer,
  },
}
</script>

<style lang="scss">
@import '../node_modules/bulma/bulma.sass';
</style>
