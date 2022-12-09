import type { CracoConfig, CracoContext } from 'craco__craco';
import type { Configuration as WebpackConfig, RuleSetRule } from 'webpack';
const Style9Plugin = require('style9/webpack');

const config: CracoConfig = {
  webpack: {
    configure: (webpackConfig: WebpackConfig, { env, paths }: CracoContext) => {
      configureStyle9(webpackConfig);
      return webpackConfig;
    },
  },
};

// This configuration was heavily inspired by https://github.com/rothsandro/vanilla-extract-cra/blob/ee100b7c4503af368d70e0527cdf81d0e7eabbc4/craco.config.js
// We've skipped some configuration for URLs/images/fonts/etc. So if there are any issues, go look there.
function configureStyle9(webpackConfig: WebpackConfig) {
  // As of 12/9/2022, CRA already includes MiniCssExtractPlugin. So we don't have to include it again.
  // Including it again create problems, like duplicate css files in build output.

  // CRA uses two rules, one with "oneOf" that handles all possible file types
  const rules = webpackConfig.module!.rules!;
  const oneOfRule: RuleSetRule = rules.find((rule): rule is RuleSetRule => rule !== '...' && rule.oneOf != null)!;

  // As of today, the Babel rule's "test" regex is the same as the one Style9's docs recommend. So it
  // should be ok to prepend the Style9 loader to the same rule. This way, Style9's processing will
  // be done before Babel transpiles files.
  const babelRule: RuleSetRule = oneOfRule.oneOf!.find(
    (rule) => rule.loader?.includes('\\babel-loader\\') ?? false
  )!;

  // The Babel rule in CRA uses the deprecated "loader" property instead of the new "use". Let's update
  // it here since we need to prepend the Style9 loader.
  babelRule.use = [
    Style9Plugin.loader,
    {
      loader: babelRule.loader,
      options: babelRule.options,
    },
  ];
  delete babelRule.loader;
  delete babelRule.options;

  webpackConfig.plugins!.push(new Style9Plugin());
}

module.exports = config;
